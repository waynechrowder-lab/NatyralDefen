using System;
using System.ComponentModel;

public static class Enums
{
    public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
    {
        var type = enumVal.GetType();
        var memInfo = type.GetMember(enumVal.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? (T)attributes[0] : null;
    }

    public static string GetDescByEnum(this Enum value)
    {
        var attribute = value.GetAttributeOfType<DescriptionAttribute>();
        return attribute == null ? value.ToString() : attribute.Description;
    }

    public static T GetValueByIndex<T>(int index)
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(index);
    }

    public static int GetIndexByValue<T>(T enumVal)
    {
        var values = Enum.GetValues(typeof(T));
        return Array.IndexOf(values, enumVal);
    }

    public static T GetEnumByDescription<T>(string description) where T : Enum
    {
        System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
            if (objs.Length > 0 && (objs[0] as DescriptionAttribute).Description == description)
            {
                return (T)field.GetValue(null);
            }
        }

        throw new ArgumentException(string.Format("{0} 未能找到对应的枚举.", description), "Description");
    }
}