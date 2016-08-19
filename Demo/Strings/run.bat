@echo off

if {%1} == {} (
echo "Runs string analysis demos"
echo "USAGE: run DOMAIN EXAMPLE [arrays] [libs]"
) else (

setlocal EnableDelayedExpansion

if "%3" == "arrays" (
set arrays=-arrays -bounds -arithmetic
)
if "%4" == "libs" (
set libs=-libpaths:..\..\Microsoft.Research\Contracts\bin\Debug\.NETFramework\v4.5
)

if not "%1" == "no" (
set domain=-strings:domain=%1
)

if "%2" == "programs" (
set file="Properties\bin\debug\Properties.exe" "StringManipulation\bin\debug\StringManipulation.exe" "QueryGeneration\bin\debug\QueryGeneration.exe"
) else if "%2" == "tests" (
set file="CharacterInclusionTests\bin\debug\CharacterInclusionTests.dll" "PrefixTests\bin\debug\PrefixTests.dll" "SuffixTests\bin\debug\SuffixTests.dll"
) else if exist "%2\bin\debug\%2.exe" (
set file="%2\bin\debug\%2.exe"
) else (
set file="%2\bin\debug\%2.dll"
)

..\..\Microsoft.Research\Clousot\bin\debug\clousot.exe -show:validations !arrays! !libs! !domain! !file!

)