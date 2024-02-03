namespace XeSharp.Serialisation.INI
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IniPropertyAttribute(string in_section = "", string in_key = "", string in_alias = "") : Attribute
    {
        /// <summary>
        /// The section containing the key for this property.
        /// </summary>
        public string Section { get; set; } = in_section;

        /// <summary>
        /// The name of the key for this property.
        /// </summary>
        public string Key { get; set; } = in_key;

        /// <summary>
        /// The alias of the key for this property.
        /// </summary>
        public string Alias { get; set; } = in_alias;
    }
}
