<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse" >

	<xsl:template match="extxref">
		<xsl:variable name="linktext">
			<xsl:choose>
				<xsl:when test=".=''">
					<xsl:value-of select="concat(@href, ': ', @targetTitle)"/>
				</xsl:when>
				<xsl:otherwise/>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="filenumber">
			<xsl:choose>
				<xsl:when test="contains(@targetDocids, ',')">
				    <!-- If more than one, take the first -->
					<xsl:value-of select="substring-before(@targetDocids, ',')"/>
				</xsl:when>
				<xsl:when test="@targetDocids!=''">
					<xsl:value-of select="@targetDocids"/>
				</xsl:when>
				<xsl:when test="contains(@href, '#')">
					<xsl:value-of select="substring-before(substring-after(@href, 'dms'), '#')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="substring-after(@href, 'dms')"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="filename" select="concat('wafResource?doc=', $filenumber, '.html')"/>
		<xsl:variable name="position" select="substring-after(@href, '#')"/>
		<xsl:variable name="filerefid" select="concat($filename, '#', $position)"/>
		<xsl:element name="a">
			<xsl:attribute name="href" select="$filerefid"/>
			<xsl:choose>
				<xsl:when test="$linktext!=''">
				   <xsl:value-of select="$linktext"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>