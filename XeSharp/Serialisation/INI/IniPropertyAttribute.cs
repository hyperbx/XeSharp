namespace XeSharp.Serialisation.INI
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IniPropertyAttribute(string in_section = "", string in_key = "", string in_alias = "") : Attribute
    {
        public string Section { get; set; } = in_section;
        public string Key { get; set; } = in_key;
        public string Alias { get; set; } = in_alias;
    }
}
