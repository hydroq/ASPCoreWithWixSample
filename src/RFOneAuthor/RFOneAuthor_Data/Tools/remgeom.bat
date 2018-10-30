java -jar "%~2\saxon9he.jar" -o:temp.x3d "%~1" "%~2\xslt\removegeometry.xsl"
move /Y temp.x3d "%~1"