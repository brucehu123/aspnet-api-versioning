﻿namespace System.Web.Http
{
    using FluentAssertions;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Xunit;
    using static System.Net.Http.HttpMethod;

    public class HttpRequestMessageExtensionsTest
    {
        [Theory]
        [InlineData( "http://localhost/Tests" )]
        [InlineData( "http://localhost/Tests?api-version=" )]
        [InlineData( "http://localhost/Tests?api-version=Alpha1" )]
        public void get_requested_api_version_should_return_null_when_query_parameter_is_nullX2C_emptyX2C_or_invalid( string requestUri )
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, requestUri );

            configuration.AddApiVersioning();
            request.SetConfiguration( configuration );

            // act
            var version = request.GetRequestedApiVersion();

            // assert
            version.Should().BeNull();
            request.Properties["MS_ApiVersion"].Should().BeNull();
        }

        [Theory]
        [InlineData( "api-version", null )]
        [InlineData( "api-version", "" )]
        [InlineData( "api-version", "Alpha1" )]
        [InlineData( "x-ms-version", null )]
        [InlineData( "x-ms-version", "" )]
        [InlineData( "x-ms-version", "Alpha1" )]
        public void get_requested_api_version_should_return_null_when_header_is_nullX2C_emptyX2C_or_invalid( string header, string value )
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage();
            var versionReader = new QueryStringOrHeaderApiVersionReader() { HeaderNames = { "api-version", "x-ms-version" } };

            configuration.AddApiVersioning( o => o.ApiVersionReader = versionReader );
            request.SetConfiguration( configuration );

            if ( value != null )
            {
                request.Headers.Add( header, value );
            }

            // act
            var version = request.GetRequestedApiVersion();

            // assert
            version.Should().BeNull();
            request.Properties["MS_ApiVersion"].Should().BeNull();
        }

        [Fact]
        public void get_requested_api_version_should_return_expected_value_from_query_parameter()
        {
            // arrange
            var requestedVersion = new ApiVersion( 1, 0 );
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, $"http://localhost/Tests?api-version={requestedVersion}" );

            configuration.AddApiVersioning();
            request.SetConfiguration( configuration );

            // act
            var version = request.GetRequestedApiVersion();

            // assert
            version.Should().Be( requestedVersion );
            request.Properties["MS_ApiVersion"].Should().Be( requestedVersion );
        }

        [Theory]
        [InlineData( "api-version" )]
        [InlineData( "x-ms-version" )]
        public void get_requested_api_version_should_return_expected_value_from_header( string headerName )
        {
            // arrange
            var requestedVersion = new ApiVersion( 1, 0 );
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage();
            var versionReader = new QueryStringOrHeaderApiVersionReader() { HeaderNames = { headerName } };

            configuration.AddApiVersioning( o => o.ApiVersionReader = versionReader );
            request.SetConfiguration( configuration );
            request.Headers.Add( headerName, requestedVersion.ToString() );

            // act
            var version = request.GetRequestedApiVersion();

            // assert
            version.Should().Be( requestedVersion );
            request.Properties["MS_ApiVersion"].Should().Be( requestedVersion );
        }
    }
}
