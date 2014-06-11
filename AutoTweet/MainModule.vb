Imports LinqToTwitter

Module MainModule

    Sub Main()

        Dim t = Task.Factory.StartNew(
            Async Function()
                Dim msg = GetMessage()
                Console.WriteLine(msg)
                Await Tweet(msg)
            End Function).Unwrap

        Try
            t.Wait()

        Catch aEx As AggregateException
            For Each e In aEx.InnerExceptions
                Console.WriteLine(e.Message)
            Next
        Catch ex As Exception
            Console.WriteLine(ex.Message)

        End Try

    End Sub

    Private Function GetMessage() As String

        Dim items = New List(Of String)

        Dim d = XDocument.Load("http://pronama.azurewebsites.net/feed/")
        For Each i In d...<item>
            Dim msg = String.Format(System.Configuration.ConfigurationManager.AppSettings("TweetFormat"), i.<title>.Value, i.<guid>.Value)

            items.Add(msg)
        Next

        Dim r = New Random
        Return items(r.Next(items.Count))

    End Function


    Async Function Tweet(msg As String) As Task

        Dim auth = SingleUserAuthorization()
        Await auth.AuthorizeAsync()

        Using twitterCtx = New TwitterContext(auth)
            Await twitterCtx.TweetAsync(msg)
        End Using

    End Function

    Private Function SingleUserAuthorization() As SingleUserAuthorizer
        Dim s = System.Configuration.ConfigurationManager.AppSettings

        Dim auth = New SingleUserAuthorizer With {
            .CredentialStore = New SingleUserInMemoryCredentialStore With
                           {
                               .ConsumerKey = s("ConsumerKey"),
                               .ConsumerSecret = s("ConsumerSecret"),
                               .AccessToken = s("AccessToken"),
                               .AccessTokenSecret = s("AccessTokenSecret")
                           }
        }
        Return auth

    End Function


End Module
