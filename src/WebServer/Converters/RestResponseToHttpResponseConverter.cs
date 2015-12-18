﻿using Devkoes.Restup.WebServer.Http;
using Devkoes.Restup.WebServer.Models.Contracts;
using Devkoes.Restup.WebServer.Models.Schemas;
using System;
using System.Linq;
using System.Text;

namespace Devkoes.Restup.WebServer.Converters
{
    internal class RestResponseToHttpResponseConverter : IRestResponseVisitor<RestRequest, IHttpResponse>
    {
        private BodySerializer _bodySerializer;

        public RestResponseToHttpResponseConverter()
        {
            _bodySerializer = new BodySerializer();
        }

        public IHttpResponse Visit(DeleteResponse response, RestRequest restReq)
        {
            var rawHttpResponseBuilder = new StringBuilder();
            rawHttpResponseBuilder.Append(CreateDefaultResponse(response));
            rawHttpResponseBuilder.Append(CreateHttpNewLine());

            return CreateHttpResponse(rawHttpResponseBuilder);
        }

        public IHttpResponse Visit(PostResponse response, RestRequest restReq)
        {
            var rawHttpResponseBuilder = new StringBuilder();
            rawHttpResponseBuilder.Append(CreateDefaultResponse(response));

            if (response.Status == PostResponse.ResponseStatus.Created)
                rawHttpResponseBuilder.Append($"Location: {response.LocationRedirect}\r\n");

            rawHttpResponseBuilder.Append(CreateHttpNewLine());

            return CreateHttpResponse(rawHttpResponseBuilder);
        }

        public IHttpResponse Visit(GetResponse response, RestRequest restReq)
        {
            return VisitWithBody(response, restReq);
        }

        public IHttpResponse Visit(PutResponse response, RestRequest restReq)
        {
            return VisitWithBody(response, restReq);
        }

        public IHttpResponse Visit(StatusOnlyResponse statusOnlyResponse, RestRequest restReq)
        {
            var rawHttpResponseBuilder = new StringBuilder();
            rawHttpResponseBuilder.Append(CreateDefaultResponse(statusOnlyResponse));
            rawHttpResponseBuilder.Append(CreateHttpNewLine());

            return CreateHttpResponse(rawHttpResponseBuilder);
        }

        public IHttpResponse Visit(MethodNotAllowedResponse methodNotAllowedResponse, RestRequest restReq)
        {
            var rawHttpResponseBuilder = new StringBuilder();
            rawHttpResponseBuilder.Append(CreateDefaultResponse(methodNotAllowedResponse));
            rawHttpResponseBuilder.AppendFormat("Allow: {0}", string.Join(",", methodNotAllowedResponse.Allows));
            rawHttpResponseBuilder.Append(CreateHttpNewLine());

            return CreateHttpResponse(rawHttpResponseBuilder);
        }

        private IHttpResponse VisitWithBody(IBodyRestResponse response, RestRequest restReq)
        {
            string bodyString = _bodySerializer.ToBody(response.BodyData, restReq);

            int bodyLength = bodyString == null ? 0 : Encoding.UTF8.GetBytes(bodyString).Length;

            var rawHttpResponseBuilder = new StringBuilder();
            rawHttpResponseBuilder.Append(CreateDefaultResponse(response));
            rawHttpResponseBuilder.AppendFormat("Content-Length: {0}\r\n", bodyLength);
            rawHttpResponseBuilder.AppendFormat("Content-Type: {0}\r\n", HttpCodesTranslator.GetMediaType(restReq.AcceptHeaders.First()));
            rawHttpResponseBuilder.Append(CreateHttpNewLine());
            rawHttpResponseBuilder.Append(bodyString);

            return CreateHttpResponse(rawHttpResponseBuilder);
        }

        private HttpResponse CreateHttpResponse(StringBuilder response)
        {
            string completeResponse = response.ToString();
            byte[] rawResponse = Encoding.UTF8.GetBytes(completeResponse);

            return new HttpResponse(completeResponse, rawResponse);
        }

        private string CreateDefaultResponse(IRestResponse response)
        {
            string statusCodeText = HttpCodesTranslator.GetHttpStatusCodeText(response.StatusCode);
            string responseStart = $"HTTP/1.1 {response.StatusCode} {statusCodeText}\r\n";
            var date = $"Date: {DateTime.Now.ToString("r")}\r\n";
            var connection = "Connection: close\r\n";

            return string.Concat(responseStart, date, connection);
        }

        private static string CreateHttpNewLine()
        {
            return "\r\n";
        }
    }
}