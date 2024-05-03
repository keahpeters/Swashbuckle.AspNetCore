using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class XmlCommentsOperationFilter : IOperationFilter
    {
        private readonly XPathNavigator _xmlNavigator;

        public XmlCommentsOperationFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            // If method is from a constructed generic type, look for comments from the generic type method
            var targetMethod = context.MethodInfo.DeclaringType.IsConstructedGenericType
                ? context.MethodInfo.GetUnderlyingGenericTypeMethod()
                : context.MethodInfo;

            if (targetMethod == null) return;

            ApplyControllerTags(operation, targetMethod.DeclaringType, context);
            ApplyMethodTags(operation, targetMethod, context);
        }

        private void ApplyControllerTags(OpenApiOperation operation, Type controllerType, OperationFilterContext context)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(controllerType);
            var responseNodes = _xmlNavigator.Select($"/doc/members/member[@name='{typeMemberName}']/response");
            ApplyResponseTags(operation, responseNodes, context);
        }

        private void ApplyMethodTags(OpenApiOperation operation, MethodInfo methodInfo, OperationFilterContext context)
        {
            var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);
            var methodNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{methodMemberName}']");

            if (methodNode == null) return;

            var summaryNode = methodNode.SelectSingleNode("summary");
            if (summaryNode != null)
                operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

            var remarksNode = methodNode.SelectSingleNode("remarks");
            if (remarksNode != null)
                operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);

            var responseNodes = methodNode.Select("response");
            ApplyResponseTags(operation, responseNodes, context);
        }

        private void ApplyResponseTags(OpenApiOperation operation, XPathNodeIterator responseNodes, OperationFilterContext context)
        {
            while (responseNodes.MoveNext())
            {
                var code = responseNodes.Current.GetAttribute("code", "");
                var contentTypes = GetResponseContentTypes(responseNodes.Current);
                var type = GetResponseType(responseNodes.Current);
                var response = operation.Responses.TryGetValue(code, out var operationResponse)
                    ? operationResponse
                    : operation.Responses[code] = new OpenApiResponse
                    {
                        Content = GetResponseContent(type, contentTypes, context)
                    };

                response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
            }
        }

        private static Dictionary<string, OpenApiMediaType> GetResponseContent(Type type, IEnumerable<string> contentTypes, OperationFilterContext context)
        {
            var content = new Dictionary<string, OpenApiMediaType>();
            var schema = type != null ? context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository) : null;

            foreach (var contentType in contentTypes)
            {
                content.Add(contentType, new OpenApiMediaType
                {
                    Schema = schema
                });
            }

            return content;
        }

        private Type GetResponseType(XPathNavigator responseNode)
        {
            string typeName = responseNode.GetAttribute("type", "");

            return Type.GetType(typeName);
        }

        private IEnumerable<string> GetResponseContentTypes(XPathNavigator responseNode)
        {
            var contentType = responseNode.GetAttribute("contentType", "");

            if (!string.IsNullOrEmpty(contentType))
            {
                return new[] { contentType };
            }

            var contentTypes = new List<string>();
            var contentTypeNodes = responseNode.Select("contentType");

            while (contentTypeNodes.MoveNext())
            {
                contentTypes.Add(contentTypeNodes.Current.InnerXml);
            }

            if (contentTypes.Any())
            {
                return contentTypes;
            }

            var exampleNodes = responseNode.Select("example");

            while (exampleNodes.MoveNext())
            {
                var exampleNodeContentType = exampleNodes.Current.GetAttribute("contentType", "");

                if (!string.IsNullOrEmpty(exampleNodeContentType))
                {
                    contentTypes.Add(exampleNodeContentType);
                }
            }

            return contentTypes;
        }
    }
}
