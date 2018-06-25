using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace SourceCampaignResponseDataFix
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("START - Update Campaign Response in Lead...");
                CrmConnection connection = CrmConnection.Parse("Url=https://fd-dev.crm.dynamics.com/; Username=FuturedonticsCRMAdmin@dentsplysirona.com; Password=XP9h#d71fdmK7&iL2;");

                OrganizationService service = new OrganizationService(connection);
                Uri oUri = new Uri("https://fd-dev.api.crm.dynamics.com/XRMServices/2011/Organization.svc");
                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = "FuturedonticsCRMAdmin@dentsplysirona.com";
                clientCredentials.UserName.Password = "XP9h#d71fdmK7&iL2";
                OrganizationServiceProxy _serviceProxy = new OrganizationServiceProxy(
                    oUri,
                    null,
                    clientCredentials,
                    null);

                execute(service, _serviceProxy);                

                Console.WriteLine("END - Update Campaign Response in Lead..");
                Console.ReadLine();

            }

            catch (FaultException<OrganizationServiceFault> ex)
            {
                string message = ex.Message;
                throw;
            }
            
        }

        static void execute(OrganizationService service, OrganizationServiceProxy _serviceProxy)
        {
            int queryCount = 50;
            int pageNumber = 1;
            QueryExpression pagequery = new QueryExpression();
            pagequery.EntityName = "lead";

            pagequery.ColumnSet = new ColumnSet(true); //retrieves all columns
            pagequery.Criteria = new FilterExpression();
            pagequery.Criteria.FilterOperator = LogicalOperator.And;
            pagequery.Criteria.AddCondition("relatedobjectid",ConditionOperator.NotNull);
            //pagequery.Criteria.AddCondition("firstname",ConditionOperator.Equal,"LogicApp8");
            pagequery.PageInfo = new PagingInfo();
            pagequery.PageInfo.Count = queryCount;
            pagequery.PageInfo.PageNumber = pageNumber;
            pagequery.PageInfo.PagingCookie = null;
            int i = 0;
            int step = 0;

            while (true)
            {
                try
                {
                    step = 1;
                    Console.WriteLine("Retrieving Page {0}", pagequery.PageInfo.PageNumber);
                    EntityCollection results = service.RetrieveMultiple(pagequery);
                    step = 2;
                    if (results.Entities.Count > 0)
                    {
                        foreach(Entity leadEntity in results.Entities)
                        {
                            try
                            {
                                step = 3;

                                Entity campaignResponse = service.Retrieve("campaignresponse", ((EntityReference)leadEntity.Attributes["relatedobjectid"]).Id, new ColumnSet(true));

                                Entity updateCR = new Entity
                                {
                                    LogicalName = "campaignresponse",
                                    Id = ((EntityReference)leadEntity.Attributes["relatedobjectid"]).Id
                                };

                                updateCR["fdx_sourcecampaignresponse"] = true;
                                updateCR["fdx_reconversionlead"] = new EntityReference("lead", leadEntity.Id);


                                step = 4;
                                service.Update(updateCR);

                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine(string.Format("error at step {0} - {1}", step, ex.ToString()));
                            }

                        }
                    }

                    if (results.MoreRecords)
                    {
                        pagequery.PageInfo.PageNumber++;
                        pagequery.PageInfo.PagingCookie = results.PagingCookie;
                    }

                    else
                    {
                        //If no more records are in the result nodes, exit the loop.
                        break;
                    }

                }

                catch(Exception ex)
                {
                    Console.WriteLine(string.Format("error at step {0} - {1}", step, ex.ToString()));
                }
            }

            Console.WriteLine("Total records created: {0}", i);            

        }

    }
}
