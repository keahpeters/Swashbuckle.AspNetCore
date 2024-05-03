namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class FakeConstructedControllerWithXmlComments : GenericControllerWithXmlComments<string>
    { }

    /// <summary>
    /// Summary for GenericControllerWithXmlComments
    /// </summary>
    public class GenericControllerWithXmlComments<T>
    {
        /// <summary>
        /// Summary for ActionWithSummaryAndRemarksTags
        /// </summary>
        /// <remarks>
        /// Remarks for ActionWithSummaryAndRemarksTags
        /// </remarks>
        public void ActionWithSummaryAndRemarksTags(T param)
        { }

        /// <param name="param1" example="Example for param1">Description for param1</param>
        /// <param name="param2" example="Example for param2">Description for param2</param>
        public void ActionWithParamTags(T param1, T param2)
        { }

        /// <response code="400" contentType="application/problem+json">description here</response>
        public void ActionWithResponseAndSingleContentTypeAttribute(T param)
        { }

        /// <response code="400">
        /// <contentType>application/problem+json</contentType>
        /// <contentType>application/problem+xml</contentType>
        /// <contentType>text/plain</contentType>
        /// </response>
        public void ActionWithResponseAndMultipleContentTypeNodes(T param)
        { }

        /// <response code="400">
        /// <example contentType="application/problem+json">Example for content type 1</example>
        /// <example contentType="application/problem+xml">Example for content type 2</example>
        /// </response>
        public void ActionWithResponseAndMultipleContentTypeAttributesInExampleNodes(T param)
        { }
    }
}
