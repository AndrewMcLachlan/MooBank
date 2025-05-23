﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:2.0.0.0
//      Reqnroll Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
using Reqnroll;
namespace Asm.MooBank.Modules.Tests.Account.Commands
{
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class UpdateAccountFeature : object, Xunit.IClassFixture<UpdateAccountFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new global::System.Globalization.CultureInfo("en-US"), "Account/Commands", "Update Account", "As a user, I want to update an account so that I can manage my money.", global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "Update.feature"
#line hidden
        
        public UpdateAccountFeature(UpdateAccountFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async global::System.Threading.Tasks.Task FeatureSetupAsync()
        {
        }
        
        public static async global::System.Threading.Tasks.Task FeatureTearDownAsync()
        {
        }
        
        public async global::System.Threading.Tasks.Task TestInitializeAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);
            try
            {
                if (((testRunner.FeatureContext != null) 
                            && (testRunner.FeatureContext.FeatureInfo.Equals(featureInfo) == false)))
                {
                    await testRunner.OnFeatureEndAsync();
                }
            }
            finally
            {
                if (((testRunner.FeatureContext != null) 
                            && testRunner.FeatureContext.BeforeFeatureHookFailed))
                {
                    throw new global::Reqnroll.ReqnrollException("Scenario skipped because of previous before feature hook error");
                }
                if ((testRunner.FeatureContext == null))
                {
                    await testRunner.OnFeatureStartAsync(featureInfo);
                }
            }
        }
        
        public async global::System.Threading.Tasks.Task TestTearDownAsync()
        {
            if ((testRunner == null))
            {
                return;
            }
            try
            {
                await testRunner.OnScenarioEndAsync();
            }
            finally
            {
                global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
                testRunner = null;
            }
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public async global::System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async global::System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
        {
            try
            {
                await this.TestInitializeAsync();
            }
            catch (System.Exception e1)
            {
                try
                {
                    ((Xunit.IAsyncLifetime)(this)).DisposeAsync();
                }
                catch (System.Exception e2)
                {
                    throw new System.AggregateException("Test initialization failed", e1, e2);
                }
                throw;
            }
        }
        
        async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
        {
            await this.TestTearDownAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Update Account")]
        [Xunit.TraitAttribute("FeatureTitle", "Update Account")]
        [Xunit.TraitAttribute("Description", "Update Account")]
        public async global::System.Threading.Tasks.Task UpdateAccount()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Update Account", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 5
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
    await testRunner.GivenAsync("I have a request to update an institution account", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 7
    await testRunner.WhenAsync("I call UpdateHandler.Handle", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 8
    await testRunner.ThenAsync("no exception is thrown", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 9
    await testRunner.AndAsync("the account is updated", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Update Account with invalid account ID")]
        [Xunit.TraitAttribute("FeatureTitle", "Update Account")]
        [Xunit.TraitAttribute("Description", "Update Account with invalid account ID")]
        public async global::System.Threading.Tasks.Task UpdateAccountWithInvalidAccountID()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Update Account with invalid account ID", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 11
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 12
    await testRunner.GivenAsync("I have a request to update an institution account", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 13
    await testRunner.AndAsync("I have an invalid account ID", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 14
    await testRunner.WhenAsync("I call UpdateHandler.Handle", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 15
    await testRunner.ThenAsync("an exception of type \'Asm.NotAuthorisedException, Asm\' is thrown", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Update Account with invalid account group ID")]
        [Xunit.TraitAttribute("FeatureTitle", "Update Account")]
        [Xunit.TraitAttribute("Description", "Update Account with invalid account group ID")]
        public async global::System.Threading.Tasks.Task UpdateAccountWithInvalidAccountGroupID()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Update Account with invalid account group ID", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 17
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 18
    await testRunner.GivenAsync("I have a request to update an institution account", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 19
    await testRunner.AndAsync("I have an invalid group ID", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 20
    await testRunner.WhenAsync("I call UpdateHandler.Handle", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 21
    await testRunner.ThenAsync("an exception of type \'Asm.NotAuthorisedException, Asm\' is thrown", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
        [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await UpdateAccountFeature.FeatureSetupAsync();
            }
            
            async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await UpdateAccountFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
