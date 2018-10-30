<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:axf="http://www.antennahouse.com/names/XSL/Extensions">
	<xsl:output method="xml" encoding="UTF-8"/>
	<!-- *********************  Sytle sets***********************-->
	<!-- Note: when running with FOP, the "Arial Black" mut be saved as "ArialBlack" according to config file 
space-before and space-after not used by Antenna (margin-top and -bottom instead)
-->
	<!-- *********************  TITLE STYLES ***********************-->
	<xsl:variable name="fontGeneral">
		<xsl:choose>
			<xsl:when test="$language = 'ja_JP'">MS Gothic</xsl:when>
			<xsl:when test="$language = 'zh_CN'">SimSun</xsl:when>
			<xsl:when test="$language = 'zh_TW'">SimSun</xsl:when>
			<xsl:when test="$language = 'th_TH'">Tahoma</xsl:when>
			<xsl:when test="$language = 'ko_KR'">Gulim</xsl:when>
			<xsl:when test="$language =('sl_SL','ru_RU','ro_RO','pl_PL','bg_BG','el_GR','cz_CZ','sk_SK','hu_HU')">Arial</xsl:when>
			<xsl:otherwise>Univers-light</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="fontTitles">Univers-light</xsl:variable>
	<xsl:attribute-set name="font">
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="body">
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
		<xsl:attribute name="font-size">8pt</xsl:attribute>
		<xsl:attribute name="font-stretch">narrower</xsl:attribute>
		<xsl:attribute name="line-height">1.3em</xsl:attribute>
		<xsl:attribute name="text-align">left</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="bodyWSM">
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
		<xsl:attribute name="font-size">9pt</xsl:attribute>
		<xsl:attribute name="line-height">1.2em</xsl:attribute>
		<xsl:attribute name="text-align">left</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="head">
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
		<xsl:attribute name="font-stretch">narrower</xsl:attribute>
		<xsl:attribute name="font-size">8pt</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="foot">
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
		<xsl:attribute name="font-size">8pt</xsl:attribute>
	</xsl:attribute-set>
	<!--************************ COVER PAGE *************************** -->
	<xsl:attribute-set name="heading">
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
		<xsl:attribute name="font-size">9pt</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="headingWSM">
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
		<xsl:attribute name="font-size">11pt</xsl:attribute>
		<xsl:attribute name="text-align">left</xsl:attribute>
	</xsl:attribute-set>
	<!--************************ COMMON STYLES*************************** -->
	<xsl:attribute-set name="all_caps">
		<xsl:attribute name="text-transform">uppercase</xsl:attribute>
		<xsl:attribute name="font-weight">bold</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="center">
		<xsl:attribute name="text-align">center</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="bold">
		<xsl:attribute name="font-weight">bold</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="italic">
		<xsl:attribute name="font-style">italic</xsl:attribute>
		<xsl:attribute name="font-size">10pt</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="prespace-s">
		<xsl:attribute name="space-before">0.3em</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="prespace">
		<xsl:attribute name="space-before">10pt</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="postspace-s">
		<xsl:attribute name="space-after">0.5em</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="postspace">
		<xsl:attribute name="space-after">9pt</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="marginbot">
		<xsl:attribute name="margin-bottom">7mm</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="keep-n">
		<xsl:attribute name="keep-with-next">always</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="keep-p">
		<xsl:attribute name="keep-with-previous">always</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="keep-p3">
		<xsl:attribute name="keep-with-previous">3</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="keep-tc-pg">
		<xsl:attribute name="keep-together.within-page">1</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="keep-tc-pg3">
		<xsl:attribute name="keep-together.within-page">3</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="keep-tc-col">
		<xsl:attribute name="keep-together.within-column">3</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="keep-line">
		<xsl:attribute name="keep-together.within-line">always</xsl:attribute>
	</xsl:attribute-set>
	<xsl:attribute-set name="TOC">
		<xsl:attribute name="font-size">14pt</xsl:attribute>
		<xsl:attribute name="font-family"><xsl:value-of select="$fontGeneral"/></xsl:attribute>
	</xsl:attribute-set>
</xsl:stylesheet>
