#!/usr/bin/env python3
"""
用法:
  python find_missing_keys.py --ref en.xml --target zh.xml --out missing.xml

默认查找 <Key> 元素。如果你的 key 在别的标签或属性里，
可用 --key-tag 改成其它标签名，或用 --key-xpath 传 XPath（例如 //Record/Key/text()）。
"""
import argparse
import xml.etree.ElementTree as ET
from pathlib import Path
import sys
import xml.dom.minidom as minidom

def extract_keys_by_tag(path: Path, tag: str):
    try:
        tree = ET.parse(str(path))
    except Exception as e:
        print(f"解析 XML 失败: {path} -> {e}", file=sys.stderr)
        return set()
    root = tree.getroot()
    # 找到所有名为 tag 的元素，取其文本（strip 后非空）
    keys = set()
    for el in root.findall(".//" + tag):
        if el.text:
            keys.add(el.text.strip())
    return keys

def pretty_xml_string(elem):
    rough = ET.tostring(elem, 'utf-8')
    reparsed = minidom.parseString(rough)
    return reparsed.toprettyxml(indent="  ", encoding='utf-8')

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--ref", required=True, help="参考文件（英文）")
    p.add_argument("--target", required=True, help="目标翻译文件（你的中文文件）")
    p.add_argument("--out", default="missing.xml", help="输出缺失 key 的 XML 模板文件")
    p.add_argument("--key-tag", default="Key", help="表示 key 的元素标签名（默认 Key）")
    args = p.parse_args()

    ref = Path(args.ref)
    target = Path(args.target)
    if not ref.exists() or not target.exists():
        print("文件不存在，请检查 --ref 和 --target 路径。", file=sys.stderr)
        sys.exit(2)

    ref_keys = extract_keys_by_tag(ref, args.key_tag)
    tgt_keys = extract_keys_by_tag(target, args.key_tag)

    missing = sorted(ref_keys - tgt_keys)

    print(f"参考 ( {ref} ) 共 {len(ref_keys)} 个 key；目标 ( {target} ) 共 {len(tgt_keys)} 个 key。")
    print(f"缺失 {len(missing)} 个 key：")
    for k in missing:
        print("  " + k)

    # 生成简单 XML 模板：根节点 <Resources>，每个缺失 key 一个 <Record><Key>..</Key><Value></Value></Record>
    root = ET.Element("Resources")
    for k in missing:
        rec = ET.SubElement(root, "Record")
        key_el = ET.SubElement(rec, args.key_tag)
        key_el.text = k
        val = ET.SubElement(rec, "Value")
        val.text = ""  # 留空给翻译填
    out_path = Path(args.out)
    try:
        pretty = pretty_xml_string(root)
        out_path.write_bytes(pretty)
        print(f"已写入模板文件：{out_path}")
    except Exception as e:
        print(f"写入输出文件失败: {e}", file=sys.stderr)

if __name__ == "__main__":
    main()
