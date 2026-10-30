namespace TestDeIa.Tests.Saas;

// Los fixtures comparten catálogos y limpian FKs globales en la misma BD de integración.
// No solapar su preparación/limpieza; las carreras de negocio se prueban explícitamente con Task.WhenAll.
[CollectionDefinition("SaasSql", DisableParallelization = true)]
public sealed class SaasSqlTestCollection;
