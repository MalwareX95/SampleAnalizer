using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SampleAnalizer.Tests
{
    public class SampleAnalyzerTests
    {
        [Fact]
        public async Task Should_Report_Diagnosticts()
        {
            var code =
                """
                    using SampleAnalizer;
                """;

            var test = new CSharpAnalyzerTest<StaticFieldAnalizer, DefaultVerifier>
            {
                TestCode = code,
                
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80.WithAssemblies([typeof(StaticField).Assembly.FullName!, typeof(StaticField).Assembly.Location])
            };

            //test.ReferenceAssemblies = test.ReferenceAssemblies.WithAssemblies();
            await test.RunAsync();



            //await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
