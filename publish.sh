dotnet pack -c Release --include-source --include-symbols  -p:SymbolPackageFormat=snupkg  --output ./package
for fileName in package/*.nupkg; do dotnet nuget push --skip-duplicate -s https://nuget.skywardapps.us/v3/index.json ${fileName}; done
for fileName in package/*.snupkg; do dotnet nuget push --skip-duplicate  -s https://nuget.skywardapps.us/v3/index.json ${fileName}; done
