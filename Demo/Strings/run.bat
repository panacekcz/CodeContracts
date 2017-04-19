@echo off

if {%1} == {} (
echo "Runs string analysis demos"
echo "USAGE: run DOMAIN EXAMPLE [/arrays] [/libs]"
) else (

setlocal EnableDelayedExpansion

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

:loop
if not "%~3" == "" (

if "%3" == "/arrays" (
set arrays=-arrays -bounds -arithmetic
) else if "%3" == "/libs" (
set libs=-libpaths:..\..\Microsoft.Research\Contracts\bin\Debug\.NETFramework\v4.5
) else if "%3" == "/trace" (
set trace=-trace:dfa
) else if "%3" == "/absolute" (
set file="%2"
) else (
echo "Unknown option %3"
set err="yes"
)
shift /3
goto :loop
)

if "%err%" == "" (
..\..\Microsoft.Research\Clousot\bin\debug\clousot.exe -show:validations !arrays! !libs! !domain! !trace! !file!
) else (
echo ..\..\Microsoft.Research\Clousot\bin\debug\clousot.exe -show:validations !arrays! !libs! !domain! !trace! !file!
)

)