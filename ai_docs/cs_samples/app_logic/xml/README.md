# XML

This sample demonstrates how to create and manipulate an *XML* document using the *Xml* class. It creates a nested *XML* tree with multiple child nodes, each containing arguments and optionally a text value.

The structure is built using the *Xml.AddChild()* method, and the arguments are parsed using *Xml.GetArgName()* and *Xml.GetArgValue()*. After construction, the *XML* tree is traversed recursively to display the structure and all attributes in the Console output.

This approach demonstrates the use of the *Xml* class for working with hierarchical data, which is useful for config files, level data, and other structured content in *XML* format.