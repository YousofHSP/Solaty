using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Common.Utilities;

public static class ModelExtensions
{
    public static string ToDisplay<T>(Expression<Func<T, object>> propertyExpression)
    {
        var memberInfo = GetPropertyInformation(propertyExpression.Body);
        if (memberInfo == null)
        {
            throw new ArgumentException(
                "No property reference expression was found.",
                "propertyExpression");
        }

        var attr = memberInfo.GetCustomAttribute<DisplayAttribute>(false);
        if (attr is null) return memberInfo.Name;
        return (attr as DisplayAttribute)?.Name ?? memberInfo.Name;
        // if (attr == null)
        // {
        //     return memberInfo.Name;
        // }
        //
        // return attr.DisplayName;
        // var propInfo = typeof(T).GetProperty(property);
        // var displayNameAttribute = propInfo?.GetCustomAttribute(typeof(DisplayAttribute));
        // if (displayNameAttribute is null) return property;
        // var displayName = (displayNameAttribute as DisplayAttribute)?.Name;
        // return displayName ?? property;
    }
    private static MemberInfo GetPropertyInformation(Expression propertyExpression)
    {
        Debug.Assert(propertyExpression != null, "propertyExpression != null");
        MemberExpression memberExpr = propertyExpression as MemberExpression;
        if (memberExpr == null)
        {
            UnaryExpression unaryExpr = propertyExpression as UnaryExpression;
            if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
            {
                memberExpr = unaryExpr.Operand as MemberExpression;
            }
        }

        if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
        {
            return memberExpr.Member;
        }

        return null;
    }
    
}