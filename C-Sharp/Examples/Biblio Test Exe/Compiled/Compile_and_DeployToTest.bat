cd ..\
csc @BiblioTests.par
cd ..\..\Source\
csc @Virtusales.Biblio.API.Par
cd ..\Portable\
copy Virtusales.Biblio.API.dll "..\Examples\Biblio Test Exe\Compiled\"
cd "..\Examples\Biblio Test Exe\Compiled"
rem APITestExe.exe