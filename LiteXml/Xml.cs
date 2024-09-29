using System.Collections;
using System.Xml;

namespace TidyHPC.LiteXml;

/// <summary>
/// Xml Wrapper
/// </summary>
public readonly struct Xml:IEnumerable<Xml>
{
    /// <summary>
    /// Create Xml from XmlNode
    /// </summary>
    /// <param name="node"></param>
    public Xml(XmlNode? node)
    {
        Node = node;
    }

    /// <summary>
    /// The XmlNode
    /// </summary>
    public XmlNode? Node { get; }

    /// <summary>
    /// Get the XmlDocument
    /// </summary>
    public XmlDocument Document
    {
        get
        {
            if (Node == null) throw new NullReferenceException();
            if (Node.OwnerDocument != null) return Node.OwnerDocument;
            if (IsDocument) return AsDocument;
            throw new NullReferenceException();
        }
    }

    #region IsAs
    /// <summary>
    /// Is Node
    /// </summary>
    public bool IsNotNull => Node != null;

    /// <summary>
    /// Is Null
    /// </summary>
    public bool IsNull => Node == null;

    /// <summary>
    /// Is Element
    /// </summary>
    public bool IsElement => Node is XmlElement;

    /// <summary>
    /// As Element
    /// </summary>
    public XmlElement AsElement => (XmlElement?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is Element Predicate
    /// </summary>
    /// <param name="onPredicate"></param>
    /// <returns></returns>
    public bool IsElementPredicate(Func<Xml, bool> onPredicate) => IsElement && onPredicate(this);

    /// <summary>
    /// Is Attribute
    /// </summary>
    public bool IsAttribute => Node?.NodeType == XmlNodeType.Attribute;

    /// <summary>
    /// As Attribute
    /// </summary>
    public XmlAttribute AsAttribute => (XmlAttribute?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is Text
    /// </summary>
    public bool IsText => Node?.NodeType == XmlNodeType.Text;

    /// <summary>
    /// As Text
    /// </summary>
    public XmlText AsText => (XmlText?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is CData
    /// </summary>
    public bool IsCData => Node?.NodeType == XmlNodeType.CDATA;

    /// <summary>
    /// As CData
    /// </summary>
    public XmlCDataSection AsCData => (XmlCDataSection?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is EntityReference
    /// </summary>
    public bool IsEntityReference => Node?.NodeType == XmlNodeType.EntityReference;

    /// <summary>
    /// As EntityReference
    /// </summary>
    public XmlEntityReference AsEntityReference => (XmlEntityReference?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is Entity
    /// </summary>
    public bool IsEntity => Node?.NodeType == XmlNodeType.Entity;

    /// <summary>
    /// As Entity
    /// </summary>
    public XmlEntity AsEntity => (XmlEntity?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is ProcessingInstruction
    /// </summary>
    public bool IsProcessingInstruction => Node?.NodeType == XmlNodeType.ProcessingInstruction;

    /// <summary>
    /// As ProcessingInstruction
    /// </summary>
    public XmlProcessingInstruction AsProcessingInstruction => (XmlProcessingInstruction?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is Comment
    /// </summary>
    public bool IsComment => Node?.NodeType == XmlNodeType.Comment;

    /// <summary>
    /// As Comment
    /// </summary>
    public XmlComment AsComment => (XmlComment?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is DocumentType
    /// </summary>
    public bool IsDocumentType => Node?.NodeType == XmlNodeType.DocumentType;

    /// <summary>
    /// As DocumentType
    /// </summary>
    public XmlDocumentType AsDocumentType => (XmlDocumentType?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is DocumentFragment
    /// </summary>
    public bool IsDocumentFragment => Node?.NodeType == XmlNodeType.DocumentFragment;

    /// <summary>
    /// As DocumentFragment
    /// </summary>
    public XmlDocumentFragment AsDocumentFragment => (XmlDocumentFragment?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is Notation
    /// </summary>
    public bool IsNotation => Node?.NodeType == XmlNodeType.Notation;

    /// <summary>
    /// As Notation
    /// </summary>
    public XmlNotation AsNotation => (XmlNotation?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is None
    /// </summary>
    public bool IsNone => Node?.NodeType == XmlNodeType.None;

    /// <summary>
    /// Is Document
    /// </summary>
    public bool IsDocument => Node?.NodeType == XmlNodeType.Document;

    /// <summary>
    /// As Document
    /// </summary>
    public XmlDocument AsDocument => (XmlDocument?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is EndElement
    /// </summary>
    public bool IsEndElement => Node?.NodeType == XmlNodeType.EndElement;

    /// <summary>
    /// Is EndEntity
    /// </summary>
    public bool IsEndEntity => Node?.NodeType == XmlNodeType.EndEntity;

    /// <summary>
    /// Is XmlDeclaration
    /// </summary>
    public bool IsXmlDeclaration => Node?.NodeType == XmlNodeType.XmlDeclaration;

    /// <summary>
    /// As XmlDeclaration
    /// </summary>
    public XmlDeclaration AsXmlDeclaration => (XmlDeclaration?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is Whitespace
    /// </summary>
    public bool IsWhitespace => Node?.NodeType == XmlNodeType.Whitespace;

    /// <summary>
    /// As Whitespace
    /// </summary>
    public XmlWhitespace AsWhitespace => (XmlWhitespace?)Node ?? throw new InvalidCastException();

    /// <summary>
    /// Is SignificantWhitespace
    /// </summary>
    public bool IsSignificantWhitespace => Node?.NodeType == XmlNodeType.SignificantWhitespace;

    /// <summary>
    /// As SignificantWhitespace
    /// </summary>
    public XmlSignificantWhitespace AsSignificantWhitespace => (XmlSignificantWhitespace?)Node ?? throw new InvalidCastException();
    #endregion

    /// <summary>
    /// The Name of Node
    /// </summary>
    public string Name
    {
        get => Node?.Name ?? string.Empty;
    }

    #region Element
    /// <summary>
    /// Get Enumerate Element
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public IEnumerator<Xml> GetEnumerator()
    {
        if (Node == null) throw new NullReferenceException();
        foreach (XmlNode node in Node)
        {
            yield return new Xml(node);
        }
    }

    /// <summary>
    /// Get Enumerate Element
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Children
    /// </summary>
    public Xml[] Children
    {
        get
        {
            if (Node is null) return [];
            List<Xml> result = [];
            foreach(XmlNode node in Node.ChildNodes)
            {
                result.Add(new Xml(node));
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// 属性的名称集合
    /// </summary>
    public string[] AttributeNames
    {
        get
        {
            if (Node is null) return [];
            if (Node.Attributes is null) return [];
            List<string> result = [];
            foreach (XmlAttribute attribute in Node.Attributes)
            {
                result.Add(attribute.Name);
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// 属性值
    /// </summary>
    public string[] AttributeValues
    {
        get
        {
            if (Node is null) return [];
            if (Node.Attributes is null) return [];
            List<string> result = [];
            foreach (XmlAttribute attribute in Node.Attributes)
            {
                result.Add(attribute.Value);
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// 属性集合
    /// </summary>
    public XmlAttribute[] Attributes
    {
        get
        {
            if (Node is null) return [];
            if (Node.Attributes is null) return [];
            List<XmlAttribute> result = [];
            foreach (XmlAttribute attribute in Node.Attributes)
            {
                result.Add(attribute);
            }
            return result.ToArray();
        }
    }

    /// <summary>
    /// Contains Attribute
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool ContainsAttribute(string name)
    {
        if (Node == null) return false;
        return Node.Attributes?[name] != null;
    }

    /// <summary>
    /// Set Attribute
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void SetAttribute(string name, string value)
    {
        if (Node == null) throw new NullReferenceException();
        if (Node.Attributes == null) throw new NullReferenceException();
        var node = Node.Attributes[name];
        if (node == null)
        {
            node = Document.CreateAttribute(name);
            Node.Attributes.Append(node);
        }
        node.Value = value;
    }

    /// <summary>
    /// 添加属性
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void AddAttribute(string name,string value)=>SetAttribute(name,value);

    /// <summary>
    /// Get Attribute
    /// </summary>
    /// <param name="name"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public string GetAttribute(string name,string defaultValue)
    {
        if (Node == null) return defaultValue;
        var node = Node.Attributes?[name];
        return node?.Value ?? defaultValue;
    }

    /// <summary>
    /// Get or Create Attribute
    /// </summary>
    /// <param name="name"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public string GetOrCreateAttribute(string name, string defaultValue)
    {
        if (Node == null) return defaultValue;
        if (Node.Attributes == null) return defaultValue;
        var node = Node.Attributes[name];
        if (node == null)
        {
            node = Document.CreateAttribute(name);
            Node.Attributes.Append(node);
            node.Value = defaultValue;
        }
        return node.Value;
    }

    /// <summary>
    /// Find Element
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Xml FirstElement(Func<Xml, bool> predicate)
    {
        if (Node == null) throw new NullReferenceException();
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (predicate(xml)) return xml;
        }
        return new Xml(null);
    }

    /// <summary>
    /// Get Element By Name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Xml GetElementByName(string name)
    {
        if (Node == null) return new Xml(null);
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (xml.Name == name) return xml;
        }
        return new Xml(null);
    }

    /// <summary>
    /// First element by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Xml FirstElementByName(string name)
    {
        if (Node == null) return new Xml(null);
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (xml.Name == name) return xml;
        }
        return new Xml(null);
    }

    /// <summary>
    /// Get Elements By Name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IEnumerable<Xml> GetElementsByName(string name)
    {
        if (Node == null) yield break;
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (xml.Name == name) yield return xml;
        }
    }

    /// <summary>
    /// Get or Create Element By Name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Xml GetOrCreateElementByName(string name)
    {
        if (Node == null) throw new NullReferenceException();
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (xml.Name == name) return xml;
        }
        var element = Document.CreateElement(name);
        Node.AppendChild(element);
        return new Xml(element);
    }

    /// <summary>
    /// Contains Element By Name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public bool ContainsElementByName(string name)
    {
        if (Node == null) throw new NullReferenceException();
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (xml.Name == name) return true;
        }
        return false;
    }

    /// <summary>
    /// Get Element By Name And Attribute
    /// </summary>
    /// <param name="name"></param>
    /// <param name="attributeName"></param>
    /// <param name="attributeValue"></param>
    /// <returns></returns>
    public bool GetElementByNameAndAttribute(string name, string attributeName, string attributeValue)
    {
        if (Node == null) return false;
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (xml.Name == name && xml.GetAttribute(attributeName, "") == attributeValue) return true;
        }
        return false;
    }

    /// <summary>
    /// Get or Create Element By Name And Attribute
    /// </summary>
    /// <param name="name"></param>
    /// <param name="attributeName"></param>
    /// <param name="attributeValue"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Xml GetOrCreateElementByNameAndAttribute(string name, string attributeName, string attributeValue)
    {
        if (Node == null) throw new NullReferenceException();
        foreach (XmlNode node in Node)
        {
            var xml = new Xml(node);
            if (!xml.IsElement) continue;
            if (xml.Name == name && xml.GetAttribute(attributeName, "") == attributeValue) return xml;
        }
        var element = Document.CreateElement(name);
        element.SetAttribute(attributeName, attributeValue);
        Node.AppendChild(element);
        return new Xml(element);
    }

    /// <summary>
    /// Get or Set InnerText
    /// </summary>
    public string InnerText
    {
        get
        {
            if (Node == null) throw new NullReferenceException();
            return Node.InnerText;
        }
        set
        {
            if (Node == null) throw new NullReferenceException();
            Node.InnerText = value;
        }
    }

    /// <summary>
    /// Get or Set InnerXml
    /// </summary>
    public string InnerXml
    {
        get
        {
            if (Node == null) throw new NullReferenceException();
            return Node.InnerXml;
        }
        set
        {
            if (Node == null) throw new NullReferenceException();
            Node.InnerXml = value;
        }
    }

    /// <summary>
    /// Get OuterXml
    /// </summary>
    public string OuterXml
    {
        get
        {
            if (Node == null) throw new NullReferenceException();
            return Node.OuterXml;
        }
    }

    /// <summary>
    /// Get Element By Index
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Xml this[string name]
    {
        get
        {
            if (Node == null) return new Xml(null);
            foreach (XmlNode node in Node)
            {
                var xml = new Xml(node);
                if (!xml.IsElement) continue;
                if (xml.Name == name) return xml;
            }
            return new Xml(null);
        }
    }

    /// <summary>
    /// Get count of the node
    /// </summary>
    public int Count => Node?.ChildNodes.Count ?? 0;

    /// <summary>
    /// Get Element By Index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Xml this[int index]
    {
        get
        {
            if (Node == null) return new Xml(null);
            if (index < 0 || index >= Node.ChildNodes.Count) return new Xml(null);
            return new Xml(Node.ChildNodes[index]);
        }
    }

    /// <summary>
    /// Add Element
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public Xml AddElement(string name)
    {
        if (Node == null) throw new NullReferenceException();
        var element = Document.CreateElement(name);
        Node.AppendChild(element);
        return new Xml(element);
    }
    #endregion

    /// <summary>
    /// Clear
    /// </summary>
    public void Clear()
    {
        if (Node == null) return;
        Node.RemoveAll();
    }

    /// <summary>
    /// Load Xml from Path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Xml Load(string path)
    {
        var doc = new XmlDocument();
        doc.Load(path);
        return new Xml(doc);
    }

    /// <summary>
    /// Try Load Xml from Path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="onDefaultValue"></param>
    /// <returns></returns>
    public static Xml TryLoad(string path,Func<Xml> onDefaultValue)
    {
        try
        {
            return Load(path);
        }
        catch
        {
            return onDefaultValue();
        }
    }

    /// <summary>
    /// Parse Xml
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public static Xml Parse(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        return new Xml(doc);
    }

    /// <summary>
    /// Try parse Xml
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="onDefaultValue"></param>
    /// <returns></returns>
    public static Xml TryParse(string xml, Func<Xml> onDefaultValue)
    {
        try
        {
            return Parse(xml);
        }
        catch
        {
            return onDefaultValue();
        }
    }

    /// <summary>
    /// New Xml
    /// </summary>
    /// <returns></returns>
    public static Xml New()
    {
        return new(new XmlDocument());
    }

    /// <summary>
    /// Save
    /// </summary>
    /// <param name="path"></param>
    public void Save(string path)
    {
        Document.Save(path);
    }

    /// <summary>
    /// Get Root
    /// </summary>
    public Xml Root => new(Document.DocumentElement);

    /// <summary>
    /// Get or Create Root
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Xml GetOrCreateRoot(string name)
    {
        if (Document.DocumentElement == null)
        {
            var element = Document.CreateElement(name);
            Document.AppendChild(element);
            return new Xml(element);
        }
        return new Xml(Document.DocumentElement);
    }

    /// <summary>
    /// Xml Document To String
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (Node == null) return string.Empty;
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace,
            Encoding = Util.UTF8
        };
        using XmlWriter writer = XmlWriter.Create(stream, settings);
        Node.WriteTo(writer);
        writer.Flush();
        return Util.UTF8.GetString(stream.ToArray());
    }
}
