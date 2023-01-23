using System;
using System.Text;
using System.Net.Http;

using DocStore.Driver.Configs;

namespace DocStore.Driver.Requesters
{
  /***
   * Generic HTTP get/post requester
   */
  public class HttpRequester : IRequestable
  {
    private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly HttpRequesterConfig _requesterConfig;
    
    public HttpRequester( HttpRequesterConfig requesterConfig )
    {
      _requesterConfig = requesterConfig;
    }
    
    public string Request( string url, int waitInMilliseconds )
    {
      if ( !Uri.IsWellFormedUriString( url, UriKind.Absolute ) )
      {
        _logger.Error( $"cannot process invalid link: {url}" );
        return null;
      }

      using var client = new HttpClient();
      
      using var requestMessage = CreateRequestMessage( url );

      if ( waitInMilliseconds > 0 )
      {
        _logger.Info( $"Waiting {waitInMilliseconds}ms before issuing HTTP request" );
        System.Threading.Thread.Sleep( waitInMilliseconds );
      }
      
      try
      {
        var response = client.SendAsync( requestMessage );
        response.Result.EnsureSuccessStatusCode();

        var contentTask = response.Result.Content.ReadAsStringAsync();
        contentTask.Wait();

        return contentTask.Result;
      }
      // AggregateException pops whenever the stream suddenly fails
      catch ( Exception e )
      {
        _logger.Error( e );
        return null;
      }
    }

    private HttpRequestMessage CreateRequestMessage( 
      string url )
    {
      var requestMessage = new HttpRequestMessage
      {
        RequestUri = new Uri( url )
      };
      
      if ( _requesterConfig.Verb.Equals( "post", StringComparison.OrdinalIgnoreCase ) )
      {
        requestMessage.Method = HttpMethod.Post;

        if ( !string.IsNullOrEmpty( _requesterConfig.Body ) )
        {
          requestMessage.Content = new StringContent( 
            _requesterConfig.Body, 
            Encoding.UTF8, 
            _requesterConfig.Format );
        }
      }
      else
        requestMessage.Method = HttpMethod.Get;
      
      foreach ( var item in _requesterConfig.Headers )
        requestMessage.Headers.Add( item.Key, item.Value );

      return requestMessage;
    }
  }
}