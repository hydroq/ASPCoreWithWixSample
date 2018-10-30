java -jar ..\saxon9he.jar -o:temp.x3d "%~1" removegeometry.xsl
move /Y temp.x3d "%~1"