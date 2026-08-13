bl_info = {
    "name": "16风格贴图渲染器（图集采样版·色板命名）",
    "author": "OpenAI",
    "version": (4, 2, 6),
    "blender": (3, 0, 0),
    "location": "View3D > 侧边栏 > 16 Style",
    "description": "直接从1024×16384图集采样16个1024×1024格子，按色板颜色名称渲染并命名",
    "category": "Render",
}

import bpy
import os
import math

from mathutils import Vector
from bpy.props import (
    PointerProperty,
    StringProperty,
    BoolProperty,
    IntProperty,
    FloatProperty,
)
from bpy.types import Operator, Panel, PropertyGroup


# 与 C# public enum 色板 完全一致的16种颜色顺序
色板 = (
    "蓝色", "灰色", "绿色", "橙色", "红色", "黄色", "白色", "黑色",
    "棕色", "卡其色", "粉色", "紫色", "黑曜石色", "银色", "青铜色", "金色",
)


def 获取颜色名称(图块索引):
    if 图块索引 < 0 or 图块索引 >= len(色板):
        raise ValueError(f"无效的色板索引：{图块索引 + 1}")
    return 色板[图块索引]


# ============================================================
# 1. 基础工具函数
# ============================================================

def 获取目标网格对象(context):
    结果 = []
    for 对象 in context.selected_objects:
        if 对象.type == 'MESH':
            结果.append(对象)
    return 结果


def 查找摄像机(scene):
    if scene.camera is not None and scene.camera.type == 'CAMERA':
        return scene.camera
    for 对象 in scene.objects:
        if 对象.type == 'CAMERA':
            scene.camera = 对象
            return 对象
    return None


def 计算世界包围盒(对象列表):
    角点列表 = []
    for 物体 in 对象列表:
        for 局部角点 in 物体.bound_box:
            世界角点 = 物体.matrix_world @ Vector(局部角点)
            角点列表.append(世界角点)
    if not 角点列表:
        raise RuntimeError("无法获取模型包围盒")
    最小x = min(点.x for 点 in 角点列表)
    最大x = max(点.x for 点 in 角点列表)
    最小y = min(点.y for 点 in 角点列表)
    最大y = max(点.y for 点 in 角点列表)
    最小z = min(点.z for 点 in 角点列表)
    最大z = max(点.z for 点 in 角点列表)
    中心 = Vector(((最小x + 最大x) * 0.5, (最小y + 最大y) * 0.5, (最小z + 最大z) * 0.5))
    尺寸 = Vector((最大x - 最小x, 最大y - 最小y, 最大z - 最小z))
    return 中心, 尺寸


def 计算投影边界(摄像机, 对象列表):
    角点列表 = []
    for 物体 in 对象列表:
        for 局部角点 in 物体.bound_box:
            世界坐标 = 物体.matrix_world @ Vector(局部角点)
            摄像机坐标 = 摄像机.matrix_world.inverted() @ 世界坐标
            角点列表.append(摄像机坐标)
    if not 角点列表:
        raise RuntimeError("无法获取模型投影范围")
    最小x = min(点.x for 点 in 角点列表)
    最大x = max(点.x for 点 in 角点列表)
    最小y = min(点.y for 点 in 角点列表)
    最大y = max(点.y for 点 in 角点列表)
    return 最小x, 最大x, 最小y, 最大y


def 调整摄像机构图(场景, 摄像机, 对象列表, 模型占画面比例):
    模型占画面比例 = max(min(模型占画面比例, 0.99), 0.01)
    最小x, 最大x, 最小y, 最大y = 计算投影边界(摄像机, 对象列表)
    宽度 = max(最大x - 最小x, 0.000001)
    高度 = max(最大y - 最小y, 0.000001)
    渲染宽 = max(场景.render.resolution_x, 1)
    渲染高 = max(场景.render.resolution_y, 1)
    宽高比 = 渲染宽 / 渲染高
    按高度计算 = 高度 / 模型占画面比例
    按宽度计算 = (宽度 / 宽高比) / 模型占画面比例
    所需垂直范围 = max(按高度计算, 按宽度计算, 0.000001)
    摄像机.data.type = 'ORTHO'
    摄像机.data.ortho_scale = 所需垂直范围
    bpy.context.view_layer.update()
    最小x, 最大x, 最小y, 最大y = 计算投影边界(摄像机, 对象列表)
    中心x = (最小x + 最大x) * 0.5
    中心y = (最小y + 最大y) * 0.5
    旋转 = 摄像机.matrix_world.to_quaternion()
    右方向 = 旋转 @ Vector((1, 0, 0))
    上方向 = 旋转 @ Vector((0, 1, 0))
    摄像机.location += 右方向 * 中心x + 上方向 * 中心y
    场景.camera = 摄像机
    bpy.context.view_layer.update()
    return 摄像机


def 设置摄像机(场景, 对象列表, 水平角, 俯仰角, 距离倍率, 模型占画面比例):
    摄像机 = 查找摄像机(场景)
    if 摄像机 is None:
        raise RuntimeError("当前场景没有摄像机，请先手动创建一个Camera")
    中心, 尺寸 = 计算世界包围盒(对象列表)
    最大尺寸 = max(尺寸.x, 尺寸.y, 尺寸.z, 0.001)
    水平弧度 = math.radians(水平角)
    俯仰弧度 = math.radians(俯仰角)
    距离 = 最大尺寸 * 距离倍率
    水平距离 = 距离 * math.cos(俯仰弧度)
    摄像机.location = Vector((
        中心.x + math.sin(水平弧度) * 水平距离,
        中心.y - math.cos(水平弧度) * 水平距离,
        中心.z + math.sin(俯仰弧度) * 距离,
    ))
    方向 = 中心 - 摄像机.location
    摄像机.rotation_euler = 方向.to_track_quat('-Z', 'Y').to_euler()
    摄像机.data.type = 'ORTHO'
    摄像机.data.clip_start = 0.001
    摄像机.data.clip_end = max(距离 * 5.0, 100.0)
    场景.camera = 摄像机
    调整摄像机构图(场景, 摄像机, 对象列表, 模型占画面比例)
    return 摄像机


# ============================================================
# 2. 16格图集采样工具（不裁剪原图）
# ============================================================

色板数量 = 16
图集行数 = 16
单格宽度 = 1024
单格高度 = 1024

自动材质名称 = "Style16_AutoMaterial"
图集图片节点名称 = "STYLE16_IMAGE_TEXTURE"
图集映射节点名称 = "STYLE16_MAPPING"
纹理坐标节点名称 = "STYLE16_TEXCOORD"
主材质节点名称 = "STYLE16_PRINCIPLED"
材质输出节点名称 = "STYLE16_MATERIAL_OUTPUT"
主光名称 = "STYLE16_Key"
辅光名称 = "STYLE16_Fill"
轮廓光名称 = "STYLE16_Rim"
正面补光名称 = "STYLE16_FrontFill"


def 检查贴图(原始贴图):
    if 原始贴图 is None:
        raise RuntimeError("请先选择16格贴图")
    宽, 高 = 原始贴图.size
    if 宽 != 单格宽度 or 高 != 图集行数 * 单格高度:
        raise RuntimeError(f"贴图尺寸必须为1024x16384，当前为{宽}x{高}")
    return True


def 设置节点位置(节点, x, y):
    节点.location = (x, y)


def 自动建立材质和灯光(context, 原始贴图, 设置):
    """为当前选中Mesh重建单材质，并建立图集采样节点与三点补光。"""
    检查贴图(原始贴图)
    对象列表 = 获取目标网格对象(context)
    if not 对象列表:
        raise RuntimeError("请先选中至少一个Mesh对象")

    创建的材质 = []
    处理过数据 = set()
    for 物体 in 对象列表:
        # 同一Mesh数据可能被多个对象共享，只需处理一次材质槽。
        数据标识 = 物体.data.as_pointer()
        if 数据标识 in 处理过数据:
            continue
        处理过数据.add(数据标识)

        材质 = bpy.data.materials.new(自动材质名称)
        材质.use_nodes = True
        树 = 材质.node_tree
        节点 = 树.nodes
        链接 = 树.links
        节点.clear()

        输出 = 节点.new('ShaderNodeOutputMaterial')
        输出.name = 材质输出节点名称
        输出.label = "Style16 输出"
        设置节点位置(输出, 520, 0)

        材质着色器 = 节点.new('ShaderNodeBsdfPrincipled')
        材质着色器.name = 主材质节点名称
        材质着色器.label = "Style16 明亮材质"
        设置节点位置(材质着色器, 220, 0)
        材质着色器.inputs['Roughness'].default_value = 0.42
        if 材质着色器.inputs.get('Specular IOR Level') is not None:
            材质着色器.inputs['Specular IOR Level'].default_value = 0.35
        elif 材质着色器.inputs.get('Specular') is not None:
            材质着色器.inputs['Specular'].default_value = 0.35

        纹理坐标 = 节点.new('ShaderNodeTexCoord')
        纹理坐标.name = 纹理坐标节点名称
        纹理坐标.label = "UV / Generated"
        设置节点位置(纹理坐标, -760, 0)

        图集映射 = 节点.new('ShaderNodeMapping')
        图集映射.name = 图集映射节点名称
        图集映射.label = "16格图集采样"
        图集映射.vector_type = 'POINT'
        设置节点位置(图集映射, -540, 0)
        sx, sy, ox, oy = 计算图集采样参数(0, True)
        图集映射.inputs['Scale'].default_value = (sx, sy, 1.0)
        图集映射.inputs['Location'].default_value = (ox, oy, 0.0)

        图集图片 = 节点.new('ShaderNodeTexImage')
        图集图片.name = 图集图片节点名称
        图集图片.label = "1024×16384 16格图集"
        图集图片.image = 原始贴图
        图集图片.interpolation = 'Linear'
        图集图片.extension = 'CLIP'
        设置节点位置(图集图片, -160, 0)

        # 有UV就用UV，没有UV则退回Generated，避免材质节点断裂。
        坐标输出 = 纹理坐标.outputs.get('UV') if 物体.data.uv_layers else 纹理坐标.outputs.get('Generated')
        if 坐标输出 is None:
            raise RuntimeError(f"物体 '{物体.name}' 无法获得UV或Generated坐标")

        链接.new(坐标输出, 图集映射.inputs['Vector'])
        链接.new(图集映射.outputs['Vector'], 图集图片.inputs['Vector'])
        链接.new(图集图片.outputs['Color'], 材质着色器.inputs['Base Color'])
        if 图集图片.outputs.get('Alpha') is not None and 材质着色器.inputs.get('Alpha') is not None:
            链接.new(图集图片.outputs['Alpha'], 材质着色器.inputs['Alpha'])
        链接.new(材质着色器.outputs['BSDF'], 输出.inputs['Surface'])

        # 颜色贴图一般作为非颜色数据采样时会发灰；这里保留原图色彩空间，
        # 让外部导入的图像设置决定最终颜色，不强制覆盖。
        物体.data.materials.clear()
        物体.data.materials.append(材质)
        创建的材质.append(材质)

    # 自动建立/更新三点灯光。以选中模型包围盒为中心，跟随模型尺寸缩放。
    中心, 尺寸 = 计算世界包围盒(对象列表)
    半径 = max(max(尺寸.x, 尺寸.y, 尺寸.z), 0.001)
    摄像机 = 查找摄像机(context.scene)
    if 摄像机 is not None:
        摄像机右方向 = 摄像机.matrix_world.to_quaternion() @ Vector((1, 0, 0))
        摄像机上方向 = 摄像机.matrix_world.to_quaternion() @ Vector((0, 1, 0))
        摄像机前方向 = 摄像机.matrix_world.to_quaternion() @ Vector((0, 0, -1))
    else:
        摄像机右方向 = Vector((1, 0, 0))
        摄像机上方向 = Vector((0, 0, 1))
        摄像机前方向 = Vector((0, -1, 0))

    def 创建或更新灯(名称, 偏移, 能量, 尺寸光):
        灯对象 = bpy.data.objects.get(名称)
        if 灯对象 is None or 灯对象.type != 'LIGHT':
            灯数据 = bpy.data.lights.new(名称, type='AREA')
            灯对象 = bpy.data.objects.new(名称, 灯数据)
            context.scene.collection.objects.link(灯对象)
        else:
            灯数据 = 灯对象.data
            灯数据.type = 'AREA'
        灯数据.energy = 能量
        灯数据.shape = 'DISK'
        灯数据.size = 尺寸光
        灯对象.location = 中心 + 偏移
        灯对象.rotation_euler = (中心 - 灯对象.location).to_track_quat('-Z', 'Y').to_euler()
        if hasattr(灯对象, 'visible_camera'):
            灯对象.visible_camera = False
        if hasattr(灯对象, 'visible_diffuse'):
            灯对象.visible_diffuse = True
        if hasattr(灯对象, 'visible_glossy'):
            灯对象.visible_glossy = True
        return 灯对象

    创建或更新灯(主光名称,
        摄像机右方向 * (半径 * 2.2) + 摄像机上方向 * (半径 * 1.8) + 摄像机前方向 * (-半径 * 1.2),
        max(260.0 * 半径 * 设置.主光强度, 0.0), 半径 * 1.8 * 设置.光源柔和度)
    创建或更新灯(辅光名称,
        摄像机右方向 * (-半径 * 1.8) + 摄像机上方向 * (半径 * 0.6) + 摄像机前方向 * (-半径 * 0.4),
        max(140.0 * 半径 * 设置.辅光强度, 0.0), 半径 * 2.2 * 设置.光源柔和度)
    创建或更新灯(轮廓光名称,
        摄像机上方向 * (半径 * 1.9) + 摄像机前方向 * (半径 * 1.5),
        max(180.0 * 半径 * 设置.轮廓光强度, 0.0), 半径 * 1.6 * 设置.光源柔和度)
    # 明确建立独立的正面补光：位于摄像机正前方、略高于模型中心，专门照亮正面。
    创建或更新灯(正面补光名称,
        摄像机前方向 * (-半径 * 2.0) + 摄像机上方向 * (半径 * 0.15),
        max(110.0 * 半径 * 设置.正面补光强度, 0.0), 半径 * 2.4 * 设置.光源柔和度)

    # 提高环境亮度，避免背面和暗部压死。
    世界 = context.scene.world
    if 世界 is None:
        世界 = bpy.data.worlds.new("Style16_World")
        context.scene.world = 世界
    世界.use_nodes = True
    背景节点 = 世界.node_tree.nodes.get('Background')
    if 背景节点 is not None:
        背景节点.inputs['Strength'].default_value = 设置.环境光强度

    context.view_layer.update()
    return 创建的材质


def 查找当前图集采样节点(对象列表, 原始贴图):
    """查找自动材质里的Mapping和Image节点，优先使用固定节点名。"""
    结果 = []
    已记录 = set()
    for 物体 in 对象列表:
        for 材质槽 in 物体.material_slots:
            材质 = 材质槽.material
            if 材质 is None or not 材质.use_nodes or 材质.node_tree is None:
                continue
            节点 = 材质.node_tree.nodes
            图集图片节点 = 节点.get(图集图片节点名称)
            图集映射节点 = 节点.get(图集映射节点名称)
            if 图集图片节点 and 图集映射节点 and 图集图片节点.type == 'TEX_IMAGE' and 图集图片节点.image == 原始贴图:
                记录标识 = (材质.name, 图集图片节点.name, 图集映射节点.name)
                if 记录标识 not in 已记录:
                    已记录.add(记录标识)
                    结果.append((材质.node_tree, 图集映射节点, 图集图片节点))
    return 结果


def 获取目标材质贴图节点(对象列表, 原始贴图):
    """只查找当前选中Mesh所使用材质里的 Image Texture 节点。"""
    节点列表 = []
    已记录 = set()
    for 物体 in 对象列表:
        for 材质槽 in 物体.material_slots:
            材质 = 材质槽.material
            if 材质 is None or not 材质.use_nodes or 材质.node_tree is None:
                continue
            for 节点 in 材质.node_tree.nodes:
                if 节点.type == 'TEX_IMAGE' and 节点.image == 原始贴图:
                    记录标识 = (材质.name, 节点.name)
                    if 记录标识 not in 已记录:
                        已记录.add(记录标识)
                        节点列表.append(节点)
    return 节点列表


def 计算图集采样参数(图块索引, 从上到下=True):
    """
    把原始UV映射到单个1024x1024格子。
    使用半像素边距，减少16格之间的双线性过滤串色。
    Blender的UV V=0位于图片底部，因此顶部第1格对应最高的tile。
    """
    if 图块索引 < 0 or 图块索引 >= 色板数量:
        raise ValueError(f"图块索引超出范围：{图块索引}，必须为0-15")

    总宽 = 1024.0
    总高 = 16384.0
    半像素_x = 0.5 / 总宽
    半像素_y = 0.5 / 总高

    if 从上到下:
        图块行号 = 图集行数 - 1 - 图块索引
    else:
        图块行号 = 图块索引

    # 输入UV 0..1 映射到当前格内部的像素中心范围。
    横向采样缩放 = (单格宽度 - 1.0) / 总宽
    纵向采样缩放 = (单格高度 - 1.0) / 总高
    横向采样偏移 = 半像素_x
    纵向采样偏移 = (图块行号 * 单格高度) / 总高 + 半像素_y
    return 横向采样缩放, 纵向采样缩放, 横向采样偏移, 纵向采样偏移


def 创建图集采样层(贴图节点, 图块索引, 从上到下):
    """设置当前Image Texture的16格采样范围。
    对自动材质优先复用固定Mapping，避免重复叠加采样层；旧材质则继续使用临时Mapping兼容。
    """
    材质树 = None
    for 材质 in bpy.data.materials:
        if 材质.use_nodes and 材质.node_tree and 贴图节点.name in 材质.node_tree.nodes:
            if 材质.node_tree.nodes.get(贴图节点.name) == 贴图节点:
                材质树 = 材质.node_tree
                break
    if 材质树 is None:
        raise RuntimeError(f"找不到纹理节点所属的材质：{贴图节点.name}")

    节点 = 材质树.nodes
    链接 = 材质树.links
    横向采样缩放, 纵向采样缩放, 横向采样偏移, 纵向采样偏移 = 计算图集采样参数(图块索引, 从上到下)
    向量输入 = 贴图节点.inputs.get('Vector')

    # 新版自动材质：直接复用固定STYLE16_MAPPING。
    固定映射 = 节点.get(图集映射节点名称)
    if (贴图节点.name == 图集图片节点名称 and 固定映射 is not None
            and 固定映射.type == 'MAPPING' and 向量输入 is not None):
        原Scale = tuple(固定映射.inputs['Scale'].default_value)
        原Location = tuple(固定映射.inputs['Location'].default_value)
        固定映射.inputs['Scale'].default_value = (横向采样缩放, 纵向采样缩放, 1.0)
        固定映射.inputs['Location'].default_value = (横向采样偏移, 纵向采样偏移, 0.0)
        原扩展 = getattr(贴图节点, 'extension', 'REPEAT')
        贴图节点.extension = 'CLIP'
        return {
            'tree': 材质树,
            '图集映射': 固定映射,
            'texcoord': None,
            '图集图片节点': 贴图节点,
            'persistent_mapping': True,
            'original_mapping_scale': 原Scale,
            'original_mapping_location': 原Location,
            'original_link': None,
            'had_original_link': True,
            'original_extension': 原扩展,
        }

    # 旧材质兼容模式：临时插入Mapping，不修改原节点结构。
    映射节点 = 节点.new('ShaderNodeMapping')
    映射节点.name = f"STYLE16_TEMP_MAPPING_{贴图节点.name}"
    映射节点.label = f"Style16 临时采样 {图块索引 + 1:02d}"
    映射节点.vector_type = 'POINT'
    映射节点.inputs['Scale'].default_value = (横向采样缩放, 纵向采样缩放, 1.0)
    映射节点.inputs['Location'].default_value = (横向采样偏移, 纵向采样偏移, 0.0)

    原有连接 = 向量输入.links[0] if 向量输入 and 向量输入.is_linked else None
    原有输入 = None
    if 原有连接 is not None:
        原有输出节点 = 原有连接.from_node
        原有输出插口 = 原有连接.from_socket
        链接.remove(原有连接)
        原有输入 = (原有输出节点, 原有输出插口)
    else:
        坐标节点 = 节点.new('ShaderNodeTexCoord')
        坐标节点.name = f"STYLE16_TEMP_TEXCOORD_{贴图节点.name}"
        坐标输出 = 坐标节点.outputs.get('UV') or 坐标节点.outputs.get('Generated')
        if 坐标输出 is None:
            节点.remove(映射节点)
            raise RuntimeError("无法创建Texture Coordinate节点")
        原有输入 = (坐标节点, 坐标输出)

    链接.new(原有输入[1], 映射节点.inputs['Vector'])
    链接.new(映射节点.outputs['Vector'], 向量输入)

    原扩展 = getattr(贴图节点, 'extension', 'REPEAT')
    贴图节点.extension = 'CLIP'
    return {
        'tree': 材质树,
        '图集映射': 映射节点,
        'texcoord': 原有输入[0] if 原有输入[0].type == 'TEX_COORD' else None,
        '图集图片节点': 贴图节点,
        'persistent_mapping': False,
        'original_link': 原有输入 if 原有连接 is not None else None,
        'had_original_link': 原有连接 is not None,
        'original_extension': 原扩展,
    }


def 恢复图集采样层(状态):
    材质树 = 状态['tree']
    链接 = 材质树.links
    节点 = 材质树.nodes
    贴图节点 = 状态['图集图片节点']

    if 状态.get('persistent_mapping'):
        映射节点 = 状态['图集映射']
        if 映射节点 and 映射节点.name in 节点:
            映射节点.inputs['Scale'].default_value = 状态['original_mapping_scale']
            映射节点.inputs['Location'].default_value = 状态['original_mapping_location']
        贴图节点.extension = 状态['original_extension']
        return

    向量输入 = 贴图节点.inputs.get('Vector')
    if 向量输入 and 向量输入.is_linked:
        for link in list(向量输入.links):
            if link.from_node == 状态['图集映射']:
                链接.remove(link)

    if 状态['had_original_link'] and 状态['original_link'] is not None:
        原节点, 原输出 = 状态['original_link']
        if 向量输入 is not None and not 向量输入.is_linked:
            链接.new(原输出, 向量输入)

    贴图节点.extension = 状态['original_extension']

    映射节点 = 状态['图集映射']
    if 映射节点 and 映射节点.name in 节点:
        节点.remove(映射节点)
    坐标节点 = 状态.get('texcoord')
    if 坐标节点 and 坐标节点.name in 节点:
        节点.remove(坐标节点)


def 建立全部图集采样层(对象列表, 原始贴图, 图块索引, 从上到下):
    纹理节点列表 = 获取目标材质贴图节点(对象列表, 原始贴图)
    if not 纹理节点列表:
        raise RuntimeError("当前选中Mesh使用的材质中，没有找到引用这张16格贴图的Image Texture节点")
    状态列表 = []
    try:
        for 纹理节点 in 纹理节点列表:
            状态列表.append(创建图集采样层(纹理节点, 图块索引, 从上到下))
    except Exception:
        for 状态 in reversed(状态列表):
            恢复图集采样层(状态)
        raise
    return 状态列表


def 恢复全部图集采样层(状态列表):
    for 状态 in reversed(状态列表):
        try:
            恢复图集采样层(状态)
        except Exception:
            pass


# ============================================================
# 3. 插件属性
# ============================================================

class STYLE16_Settings(PropertyGroup):
    贴图: PointerProperty(
        name="16格贴图",
        type=bpy.types.Image,
        description="尺寸必须为1024x16384的纵向拼合贴图",
    )
    输出目录: StringProperty(name="输出目录", subtype='DIR_PATH', default="//style_render/")
    从上到下编号: BoolProperty(name="从图片顶部开始编号", default=True)
    渲染宽度: IntProperty(name="宽度", default=128, min=1, max=8192)
    渲染高度: IntProperty(name="高度", default=128, min=1, max=8192)
    风格数量: IntProperty(name="风格数量", default=16, min=1, max=16)
    水平侧视角: FloatProperty(name="水平侧视角", default=15.0, min=-45.0, max=45.0)
    垂直俯仰角: FloatProperty(name="垂直俯仰角", default=5.0, min=-45.0, max=45.0)
    相机距离倍率: FloatProperty(name="相机距离倍率", default=3.0, min=0.1, max=20.0)
    模型占画面比例: FloatProperty(name="模型占画面比例", default=0.80, min=0.01, max=0.99, subtype='PERCENTAGE')
    自动调整摄像机: BoolProperty(name="自动调整摄像机", default=True)
    透明背景: BoolProperty(name="透明背景", default=False)
    背景颜色: bpy.props.FloatVectorProperty(
        name="背景颜色", subtype='COLOR', size=4, min=0.0, max=1.0, default=(0.05, 0.05, 0.05, 1.0)
    )
    主光强度: FloatProperty(name="主光强度", default=0.25, min=0.0, max=2.0, subtype='FACTOR')
    辅光强度: FloatProperty(name="辅光强度", default=0.12, min=0.0, max=2.0, subtype='FACTOR')
    轮廓光强度: FloatProperty(name="轮廓光强度", default=0.18, min=0.0, max=2.0, subtype='FACTOR')
    正面补光强度: FloatProperty(name="正面补光强度", default=0.10, min=0.0, max=2.0, subtype='FACTOR')
    环境光强度: FloatProperty(name="环境光强度", default=0.06, min=0.0, max=1.0, subtype='FACTOR')
    光源柔和度: FloatProperty(name="光源柔和度", default=1.0, min=0.2, max=3.0, subtype='FACTOR')


# ============================================================
# 4. 操作符：立即调整摄像机
# ============================================================

class STYLE16_OT_ApplyLighting(Operator):
    bl_idname = "style16.apply_lighting"
    bl_label = "应用补光设置"
    bl_description = "按照当前UI参数更新四点灯光和环境光"

    def execute(self, context):
        设置 = context.scene.style16_settings
        try:
            对象列表 = 获取目标网格对象(context)
            if not 对象列表:
                raise RuntimeError("请先选中至少一个Mesh对象")
            中心, 尺寸 = 计算世界包围盒(对象列表)
            半径 = max(max(尺寸.x, 尺寸.y, 尺寸.z), 0.001)
            摄像机 = 查找摄像机(context.scene)
            if 摄像机 is not None:
                摄像机右方向 = 摄像机.matrix_world.to_quaternion() @ Vector((1, 0, 0))
                摄像机上方向 = 摄像机.matrix_world.to_quaternion() @ Vector((0, 1, 0))
                摄像机前方向 = 摄像机.matrix_world.to_quaternion() @ Vector((0, 0, -1))
            else:
                摄像机右方向 = Vector((1, 0, 0))
                摄像机上方向 = Vector((0, 0, 1))
                摄像机前方向 = Vector((0, -1, 0))

            def 创建或更新灯光(灯光名称, 偏移, 能量, 尺寸):
                灯光对象 = bpy.data.objects.get(灯光名称)
                if 灯光对象 is None or 灯光对象.type != 'LIGHT':
                    灯光数据 = bpy.data.lights.new(灯光名称, type='AREA')
                    灯光对象 = bpy.data.objects.new(灯光名称, 灯光数据)
                    context.scene.collection.objects.link(灯光对象)
                灯光对象.data.type = 'AREA'
                灯光对象.data.energy = max(能量, 0.0)
                灯光对象.data.shape = 'DISK'
                灯光对象.data.size = max(尺寸, 0.01)
                灯光对象.location = 中心 + 偏移
                灯光对象.rotation_euler = (中心 - 灯光对象.location).to_track_quat('-Z', 'Y').to_euler()
                灯光对象.hide_viewport = False
                灯光对象.hide_render = False
                if hasattr(灯光对象, 'visible_camera'):
                    灯光对象.visible_camera = False
                return 灯光对象

            创建或更新灯光(主光名称, 摄像机右方向 * (半径 * 2.2) + 摄像机上方向 * (半径 * 1.8) + 摄像机前方向 * (-半径 * 1.2), 260.0 * 半径 * 设置.主光强度, 半径 * 1.8 * 设置.光源柔和度)
            创建或更新灯光(辅光名称, 摄像机右方向 * (-半径 * 1.8) + 摄像机上方向 * (半径 * 0.6) + 摄像机前方向 * (-半径 * 0.4), 140.0 * 半径 * 设置.辅光强度, 半径 * 2.2 * 设置.光源柔和度)
            创建或更新灯光(轮廓光名称, 摄像机上方向 * (半径 * 1.9) + 摄像机前方向 * (半径 * 1.5), 180.0 * 半径 * 设置.轮廓光强度, 半径 * 1.6 * 设置.光源柔和度)
            # 正面补光始终朝向模型中心，位置与摄像机视线同轴略偏上。
            创建或更新灯光(正面补光名称, 摄像机前方向 * (-半径 * 2.0) + 摄像机上方向 * (半径 * 0.15), 110.0 * 半径 * 设置.正面补光强度, 半径 * 2.4 * 设置.光源柔和度)

            世界 = context.scene.world
            if 世界 is not None and 世界.use_nodes:
                背景节点 = 世界.node_tree.nodes.get('Background')
                if 背景节点 is not None:
                    背景节点.inputs['Strength'].default_value = 设置.环境光强度
            context.view_layer.update()
        except Exception as e:
            self.report({'ERROR'}, f"应用补光失败：{e}")
            return {'CANCELLED'}
        self.report({'INFO'}, "补光设置已应用")
        return {'FINISHED'}


class STYLE16_OT_SelectFrontFill(Operator):
    bl_idname = "style16.select_front_fill"
    bl_label = "选择正面补光"
    bl_description = "在视图中选择并定位到STYLE16正面补光灯"

    def execute(self, context):
        灯光对象 = bpy.data.objects.get(正面补光名称)
        if 灯光对象 is None or 灯光对象.type != 'LIGHT':
            self.report({'ERROR'}, "尚未创建正面补光，请先点击“自动整理材质 + 创建补光”或“应用补光设置”")
            return {'CANCELLED'}
        for 对象 in context.selected_objects:
            对象.select_set(False)
        灯光对象.select_set(True)
        context.view_layer.objects.active = 灯光对象
        return {'FINISHED'}


class STYLE16_OT_SetupMaterial(Operator):
    bl_idname = "style16.setup_material"
    bl_label = "自动整理材质 + 创建补光"
    bl_description = "删除选中Mesh多余材质槽，重建16格图集材质，并自动创建四点灯光"

    def execute(self, context):
        设置 = context.scene.style16_settings
        try:
            自动建立材质和灯光(context, 设置.贴图, 设置)
        except Exception as e:
            self.report({'ERROR'}, f"材质/灯光设置失败：{e}")
            return {'CANCELLED'}
        self.report({'INFO'}, "已重建选中Mesh材质，并创建/更新四点补光")
        return {'FINISHED'}


class STYLE16_OT_AdjustCamera(Operator):
    bl_idname = "style16.adjust_camera"
    bl_label = "立即调整摄像机"

    def execute(self, context):
        设置 = context.scene.style16_settings
        场景 = context.scene
        物体列表 = 获取目标网格对象(context)
        if not 物体列表:
            self.report({'ERROR'}, "请先选中需要调整取景的Mesh对象")
            return {'CANCELLED'}
        摄像机 = 查找摄像机(场景)
        if 摄像机 is None:
            self.report({'ERROR'}, "当前场景没有摄像机，请先手动创建Camera")
            return {'CANCELLED'}
        旧宽度 = 场景.render.resolution_x
        旧高度 = 场景.render.resolution_y
        旧百分比 = 场景.render.resolution_percentage
        try:
            # “立即调整摄像机”也按插件设置的最终输出比例计算。
            场景.render.resolution_x = 设置.渲染宽度
            场景.render.resolution_y = 设置.渲染高度
            场景.render.resolution_percentage = 100
            设置摄像机(场景, 物体列表, 设置.水平侧视角, 设置.垂直俯仰角, 设置.相机距离倍率, 设置.模型占画面比例)
        except Exception as e:
            self.report({'ERROR'}, f"调整摄像机失败：{e}")
            return {'CANCELLED'}
        finally:
            场景.render.resolution_x = 旧宽度
            场景.render.resolution_y = 旧高度
            场景.render.resolution_percentage = 旧百分比
        context.view_layer.update()
        self.report({'INFO'}, f"摄像机已调整：水平 {设置.水平侧视角:.1f}°，垂直 {设置.垂直俯仰角:.1f}°，模型占画面 {设置.模型占画面比例 * 100:.1f}%")
        return {'FINISHED'}


# ============================================================
# 4. 操作符：渲染全部风格
# ============================================================

class STYLE16_OT_Render(Operator):
    bl_idname = "style16.render"
    bl_label = "渲染全部16种风格"

    def execute(self, context):
        设置 = context.scene.style16_settings
        场景 = context.scene

        if 场景.render.engine == 'BLENDER_WORKBENCH':
            self.report({'WARNING'}, "当前渲染引擎为Workbench，将无法显示纹理！请切换为Eevee或Cycles")

        物体列表 = 获取目标网格对象(context)
        if not 物体列表:
            self.report({'ERROR'}, "请先选中需要渲染的Mesh对象")
            return {'CANCELLED'}

        for 物体 in 物体列表:
            if not 物体.data.uv_layers:
                self.report({'WARNING'}, f"物体 '{物体.name}' 没有UV层，贴图可能无法正确采样")

        原始贴图 = 设置.贴图
        try:
            检查贴图(原始贴图)
        except Exception as e:
            self.report({'ERROR'}, str(e))
            return {'CANCELLED'}

        摄像机 = 查找摄像机(场景)
        if 摄像机 is None:
            self.report({'ERROR'}, "当前场景没有摄像机，请先手动创建Camera")
            return {'CANCELLED'}

        输出路径 = bpy.path.abspath(设置.输出目录)
        os.makedirs(输出路径, exist_ok=True)

        旧宽度 = 场景.render.resolution_x
        旧高度 = 场景.render.resolution_y
        旧百分比 = 场景.render.resolution_percentage
        旧文件路径 = 场景.render.filepath
        旧透明背景 = 场景.render.film_transparent
        旧世界 = 场景.world
        旧世界颜色 = 旧世界.color[:] if 旧世界 is not None else None
        临时世界 = None

        场景.render.resolution_x = 设置.渲染宽度
        场景.render.resolution_y = 设置.渲染高度
        场景.render.resolution_percentage = 100

        # 必须在设置最终输出宽高之后再调整正交相机，
        # 否则16:9 / 1:1 / 4:3等比例会导致取景尺度计算错误。
        if 设置.自动调整摄像机:
            try:
                设置摄像机(场景, 物体列表, 设置.水平侧视角, 设置.垂直俯仰角, 设置.相机距离倍率, 设置.模型占画面比例)
            except Exception as e:
                self.report({'ERROR'}, f"调整摄像机失败：{e}")
                return {'CANCELLED'}

        场景.render.film_transparent = 设置.透明背景
        if not 设置.透明背景:
            if 场景.world is None:
                临时世界 = bpy.data.worlds.new("Style16_TempWorld")
                场景.world = 临时世界
            场景.world.color = 设置.背景颜色[:3]

        状态列表 = []
        try:
            for 索引 in range(设置.风格数量):
                状态列表 = 建立全部图集采样层(物体列表, 原始贴图, 索引, 设置.从上到下编号)
                文件名 = f"{获取颜色名称(索引)}.png"
                场景.render.filepath = os.path.join(输出路径, 文件名)
                场景.camera = 摄像机
                context.view_layer.update()
                self.report({'INFO'}, f"正在渲染 {索引+1}/{设置.风格数量}: {文件名}")
                bpy.ops.render.render('EXEC_DEFAULT', write_still=True, scene=场景.name)
                恢复全部图集采样层(状态列表)
                状态列表 = []

        except Exception as e:
            self.report({'ERROR'}, f"渲染过程中发生错误：{e}")
            return {'CANCELLED'}
        finally:
            恢复全部图集采样层(状态列表)
            场景.render.resolution_x = 旧宽度
            场景.render.resolution_y = 旧高度
            场景.render.resolution_percentage = 旧百分比
            场景.render.filepath = 旧文件路径
            场景.render.film_transparent = 旧透明背景
            if 旧世界 is None:
                场景.world = None
                if 临时世界 is not None:
                    try:
                        bpy.data.worlds.remove(临时世界)
                    except Exception:
                        pass
            else:
                场景.world = 旧世界
                if 旧世界颜色 is not None:
                    场景.world.color = 旧世界颜色

        self.report({'INFO'}, f"完成：共生成 {设置.风格数量} 张图片（未复制/裁剪原始贴图）")
        return {'FINISHED'}


# ============================================================
# 5. 操作符：渲染单张风格（测试用）
# ============================================================

class STYLE16_OT_RenderOne(Operator):
    bl_idname = "style16.render_one"
    bl_label = "渲染当前色板"
    图块索引: IntProperty(name="色板编号", default=1, min=1, max=16)

    def execute(self, context):
        设置 = context.scene.style16_settings
        场景 = context.scene

        if 场景.render.engine == 'BLENDER_WORKBENCH':
            self.report({'WARNING'}, "当前渲染引擎为Workbench，将无法显示纹理！请切换为Eevee或Cycles")

        物体列表 = 获取目标网格对象(context)
        if not 物体列表:
            self.report({'ERROR'}, "请先选中Mesh对象")
            return {'CANCELLED'}

        原始贴图 = 设置.贴图
        try:
            检查贴图(原始贴图)
        except Exception as e:
            self.report({'ERROR'}, str(e))
            return {'CANCELLED'}

        摄像机 = 查找摄像机(场景)
        if 摄像机 is None:
            self.report({'ERROR'}, "当前场景没有摄像机，请先手动创建Camera")
            return {'CANCELLED'}

        输出路径 = bpy.path.abspath(设置.输出目录)
        os.makedirs(输出路径, exist_ok=True)

        旧宽度 = 场景.render.resolution_x
        旧高度 = 场景.render.resolution_y
        旧百分比 = 场景.render.resolution_percentage
        旧文件路径 = 场景.render.filepath
        旧透明背景 = 场景.render.film_transparent
        旧世界 = 场景.world
        旧世界颜色 = 旧世界.color[:] if 旧世界 is not None else None
        临时世界 = None
        状态列表 = []

        场景.render.resolution_x = 设置.渲染宽度
        场景.render.resolution_y = 设置.渲染高度
        场景.render.resolution_percentage = 100

        if 设置.自动调整摄像机:
            try:
                设置摄像机(场景, 物体列表, 设置.水平侧视角, 设置.垂直俯仰角, 设置.相机距离倍率, 设置.模型占画面比例)
            except Exception as e:
                self.report({'ERROR'}, f"调整摄像机失败：{e}")
                return {'CANCELLED'}

        场景.render.film_transparent = 设置.透明背景
        if not 设置.透明背景:
            if 场景.world is None:
                临时世界 = bpy.data.worlds.new("Style16_TempWorld")
                场景.world = 临时世界
            场景.world.color = 设置.背景颜色[:3]

        try:
            状态列表 = 建立全部图集采样层(物体列表, 原始贴图, self.图块索引 - 1, 设置.从上到下编号)
            文件名 = f"{获取颜色名称(self.图块索引 - 1)}.png"
            场景.render.filepath = os.path.join(输出路径, 文件名)
            场景.camera = 摄像机
            context.view_layer.update()
            bpy.ops.render.render('EXEC_DEFAULT', write_still=True, scene=场景.name)
        except Exception as e:
            self.report({'ERROR'}, f"渲染出错：{e}")
            return {'CANCELLED'}
        finally:
            恢复全部图集采样层(状态列表)
            场景.render.resolution_x = 旧宽度
            场景.render.resolution_y = 旧高度
            场景.render.resolution_percentage = 旧百分比
            场景.render.filepath = 旧文件路径
            场景.render.film_transparent = 旧透明背景
            if 旧世界 is None:
                场景.world = None
                if 临时世界 is not None:
                    try:
                        bpy.data.worlds.remove(临时世界)
                    except Exception:
                        pass
            else:
                场景.world = 旧世界
                if 旧世界颜色 is not None:
                    场景.world.color = 旧世界颜色

        self.report({'INFO'}, f"已渲染色板：{获取颜色名称(self.图块索引 - 1)}")
        return {'FINISHED'}


# ============================================================
# 7. UI面板
# ============================================================

class STYLE16_PT_Main(Panel):
    bl_label = "16种风格批量渲染"
    bl_idname = "STYLE16_PT_Main"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "16 Style"

    def draw(self, context):
        布局 = self.layout
        设置 = context.scene.style16_settings
        场景 = context.scene

        盒子 = 布局.box()
        盒子.label(text="摄像机")
        摄像机 = 查找摄像机(场景)
        if 摄像机:
            盒子.label(text=f"当前摄像机：{摄像机.name}", icon='CAMERA_DATA')
        else:
            盒子.label(text="请先手动创建Camera", icon='ERROR')
        盒子.prop(设置, "自动调整摄像机")
        if 设置.自动调整摄像机:
            盒子.prop(设置, "水平侧视角")
            盒子.prop(设置, "垂直俯仰角")
            盒子.prop(设置, "相机距离倍率")
            盒子.prop(设置, "模型占画面比例")
            行 = 盒子.row()
            行.scale_y = 1.3
            行.operator("style16.adjust_camera", text="立即调整摄像机", icon='CAMERA_DATA')

        盒子 = 布局.box()
        盒子.label(text="背景")
        盒子.prop(设置, "透明背景")
        if not 设置.透明背景:
            盒子.prop(设置, "背景颜色")

        盒子 = 布局.box()
        盒子.label(text="16格图集（直接采样，不裁剪）")
        盒子.prop(设置, "贴图")
        if 设置.贴图:
            图 = 设置.贴图
            if 图.size[0] == 1024 and 图.size[1] == 16384:
                盒子.label(text="尺寸正确：1024 x 16384", icon='CHECKMARK')
            else:
                盒子.label(text=f"尺寸错误：{图.size[0]} x {图.size[1]}", icon='ERROR')

        盒子 = 布局.box()
        盒子.label(text="目标对象")
        盒子.label(text="使用当前选中的Mesh对象")
        行 = 盒子.row()
        行.scale_y = 1.25
        行.operator("style16.setup_material", text="自动整理材质 + 创建补光", icon='MATERIAL')
        盒子.label(text="会删除多余材质槽，并自动引用上方16格图集", icon='INFO')

        盒子 = 布局.box()
        盒子.label(text="补光控制")
        盒子.prop(设置, "主光强度", slider=True)
        盒子.prop(设置, "辅光强度", slider=True)
        盒子.prop(设置, "轮廓光强度", slider=True)
        盒子.prop(设置, "正面补光强度", slider=True)
        盒子.prop(设置, "环境光强度", slider=True)
        盒子.label(text="正面补光位于摄像机正前方，用于提亮模型正面", icon='LIGHT')
        盒子.prop(设置, "光源柔和度", slider=True)
        行 = 盒子.row()
        行.scale_y = 1.2
        行.operator("style16.apply_lighting", text="应用补光设置", icon='LIGHT')
        行 = 盒子.row(align=True)
        行.operator("style16.select_front_fill", text="选择正面补光", icon='LIGHT')
        盒子.label(text="选中后可在3D视图中看到灯的位置并手动移动", icon='INFO')

        盒子 = 布局.box()
        盒子.label(text="图集编号方向")
        盒子.prop(设置, "从上到下编号")

        盒子 = 布局.box()
        盒子.label(text="输出")
        盒子.prop(设置, "输出目录")
        行 = 盒子.row(align=True)
        行.prop(设置, "渲染宽度")
        行.prop(设置, "渲染高度")
        盒子.prop(设置, "风格数量")

        布局.separator()
        行 = 布局.row()
        行.scale_y = 1.5
        行.operator("style16.render", text="渲染全部16种风格", icon='RENDER_STILL')

        布局.separator()
        盒子 = 布局.box()
        盒子.label(text="单张测试（按色板名称）")
        分组 = [range(1,5), range(5,9), range(9,13), range(13,17)]
        for 组 in 分组:
            行 = 盒子.row(align=True)
            for 编号 in 组:
                颜色名称 = 获取颜色名称(编号 - 1)
                操作 = 行.operator("style16.render_one", text=颜色名称)
                操作.图块索引 = 编号


# ============================================================
# 8. 注册与注销
# ============================================================

classes = (
    STYLE16_Settings,
    STYLE16_OT_ApplyLighting,
    STYLE16_OT_SelectFrontFill,
    STYLE16_OT_SetupMaterial,
    STYLE16_OT_AdjustCamera,
    STYLE16_OT_Render,
    STYLE16_OT_RenderOne,
    STYLE16_PT_Main,
)

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.style16_settings = PointerProperty(type=STYLE16_Settings)

def unregister():
    if hasattr(bpy.types.Scene, "style16_settings"):
        del bpy.types.Scene.style16_settings
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)

if __name__ == "__main__":
    register()