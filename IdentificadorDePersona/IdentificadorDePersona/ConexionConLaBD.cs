using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Threading.Tasks;
using System;

namespace IdentificadorDePersona
{
    class ConexionConLaBD
    {

        static DocumentClient docClient = null;
        static readonly string databaseName = "GestorDePersonas";
        static readonly string collectionName = "Persona";

        public static async Task<bool> Initialize()
        {

            string URLdeConexion = "https://basededatosazureexamen.documents.azure.com:443/";
            String claveDeConexion = "WwnXFdqKvsZRfT7XhkXM2aeoBK3YQVuY9NYhpqpnWmoIkZMpGJLsrPpn65kou99CNJoJSC0FLJF7BxB2aISypA==";

            if (docClient != null)
                return true;

            try
            {
                docClient = new DocumentClient(new Uri(URLdeConexion), claveDeConexion);

                // Create the database - this can also be done through the portal
                await docClient.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });

                // Create the collection - make sure to specify the RUs - has pricing implications
                // This can also be done through the portal

                await docClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseName),
                    new DocumentCollection { Id = collectionName },
                    new RequestOptions { OfferThroughput = 400 }
                );

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                docClient = null;

                return false;
            }

            return true;
        }



        public async static Task<Persona> GetPersona(int identificacion)
        {
            var todos = new List<Persona>();

            if (!await Initialize())
                return null;

            var Query = docClient.CreateDocumentQuery<Persona>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                  .AsDocumentQuery();

            while (Query.HasMoreResults)
            {
                var queryResults = await Query.ExecuteNextAsync<Persona>();

                todos.AddRange(queryResults);
            }

            foreach (var persona in todos)
            {
                if (persona.Identificacion == identificacion)
                {
                    return persona;
                }
            }

            return null;
        }

    }
}
