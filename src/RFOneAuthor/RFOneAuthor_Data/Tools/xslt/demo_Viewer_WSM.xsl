<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
<!ENTITY nbsp "&#160;">
<!ENTITY reg "&#174;">
<!ENTITY mdash "&#8212;">
]>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:exsl="http://exslt.org/common" xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse">

	<!-- VERSION 20111222 -->

	<!-- set this parameter for XMLCompass use or for developer use
	<xsl:param name="baseInput">C:/SPX/Ducati/sampleInput/</xsl:param>-->

	<!-- set this parameter for XMLCompass use or for developer use-->
	<!-- 	<xsl:param name="baseOutput">C:/SPX/Ducati/sampleOutput/test/</xsl:param>-->
	<xsl:param name="baseOutput"></xsl:param>

	<!-- two params come from the command line -->
	<xsl:param name="outputDir"></xsl:param>
	<xsl:param name="curlang"></xsl:param>

	<!-- this makes output directory names for any language -->
	<xsl:variable name="lang_dir">
		<xsl:choose>
			<xsl:when test="$curlang = 'en_US' ">USA</xsl:when>
			<xsl:when test="$curlang = 'en_UK' ">ENG</xsl:when>
			<xsl:when test="$curlang = 'fr_FR' ">FRA</xsl:when>
			<xsl:when test="$curlang = 'fr_CA' ">FRC</xsl:when>
			<xsl:when test="$curlang = 'de_DE' ">DEU</xsl:when>
			<xsl:when test="$curlang = 'it_IT' ">ITA</xsl:when>
			<xsl:when test="$curlang = 'es_ES' ">SPA</xsl:when>
			<xsl:when test="$curlang = 'pt_PT' ">POR</xsl:when>
			<xsl:when test="$curlang = 'zh_CN' ">CIN</xsl:when>
			<xsl:when test="$curlang = 'zh_TW' ">TWA</xsl:when>
			<xsl:when test="$curlang = 'ja_JP' ">JPN</xsl:when>
			<xsl:when test="$curlang = 'ko_KR' ">KOR</xsl:when>
			<xsl:when test="$curlang = 'th_TH' ">THA</xsl:when>
			<xsl:when test="$curlang = 'el_GR' ">GRE</xsl:when>
			<xsl:when test="$curlang = 'nl_NL' ">DUT</xsl:when>
			<xsl:when test="$curlang = 'en_us' ">USA</xsl:when>
			<xsl:when test="$curlang = 'en_uk' ">ENG</xsl:when>
			<xsl:when test="$curlang = 'fr_fr' ">FRA</xsl:when>
			<xsl:when test="$curlang = 'fr_ca' ">FRC</xsl:when>
			<xsl:when test="$curlang = 'de_de' ">DEU</xsl:when>
			<xsl:when test="$curlang = 'it_it' ">ITA</xsl:when>
			<xsl:when test="$curlang = 'es_es' ">SPA</xsl:when>
			<xsl:when test="$curlang = 'pt_pt' ">POR</xsl:when>
			<xsl:when test="$curlang = 'zh_cn' ">CIN</xsl:when>
			<xsl:when test="$curlang = 'zh_tw' ">TWA</xsl:when>
			<xsl:when test="$curlang = 'ja_jp' ">JPN</xsl:when>
			<xsl:when test="$curlang = 'ko_kr' ">KOR</xsl:when>
			<xsl:when test="$curlang = 'th_th' ">THA</xsl:when>
			<xsl:when test="$curlang = 'el_gr' ">GRE</xsl:when>
			<xsl:when test="$curlang = 'nl_nl' ">DUT</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$curlang"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- this corrects for known casing issues in curlang parameters-->
	<xsl:variable name="language">
		<xsl:choose>
			<xsl:when test="contains($curlang, '_')">
				<xsl:choose>
					<xsl:when test="$curlang = 'en_us' ">en_US</xsl:when>
					<xsl:when test="$curlang = 'en_uk' ">en_UK</xsl:when>
					<xsl:when test="$curlang = 'zh_cn' ">zh_CN</xsl:when>
					<xsl:when test="$curlang = 'zh_tw' ">zh_TW</xsl:when>
					<xsl:when test="$curlang = 'it_it' ">it_IT</xsl:when>
					<xsl:when test="$curlang = 'fr_ca' ">fr_CA</xsl:when>
					<xsl:when test="$curlang = 'fr_fr' ">fr_FR</xsl:when>
					<xsl:when test="$curlang = 'de_de' ">de_DE</xsl:when>
					<xsl:when test="$curlang = 'es_es' ">es_ES</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$curlang"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$curlang"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:strip-space elements="*"/>

	<xsl:include href="lang/lang.xss"/>

	<xsl:template match="WSM">
		<xsl:param name="family" select="@family"/>
		<xsl:param name="outputDir" select="$outputDir"/>
		<xsl:param name="uid" select="string:replace(uuid:to-string(uuid:random-uUID()), '-', '')" xmlns:uuid="java:java.util.UUID" xmlns:string="java:java.lang.String"/>
		<!-- <xsl:param name="outputName" select="concat($family,'_info_', $curlang, '.xml')"/> -->
		<xsl:param name="outputName" select="concat('wsm_info_', $uid, '_', $curlang, '.xml')"/>
		<xsl:param name="outputPath" select="concat($baseOutput, $outputDir, '/wsm/', $outputName)"/>
		<!--xsl:variable name="boilerFile" select="concat('lang/boiler_', $language, '.xml')"/-->
		<!-- create info.xml at top level for any language -->

		<xsl:result-document method="xml" href="file:///{$outputPath}" indent="yes" omit-xml-declaration="no">
			<xsl:element name="info">
				<xsl:for-each select="//topic">
					<xsl:element name="mdcomplex">
						<xsl:attribute name="name" select=" 'Topic' "/>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'docdesc' "/>
							<xsl:attribute name="value" select="@docdesc"/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'sectioncode' "/>
							<xsl:attribute name="value" select="@sectionCode"/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'chaptercode' "/>
							<xsl:attribute name="value" select="@chapterCode"/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'topiccode' "/>
							<xsl:attribute name="value" select="@topicCode"/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'doctype' "/>
							<xsl:attribute name="value" select=" 'topic' "/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'docext' "/>
							<xsl:attribute name="value" select=" 'xml' "/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'docid' "/>
							<xsl:attribute name="value" select="@vf:dmsid"/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'langcountry' "/>
							<xsl:attribute name="value" select="$curlang"/>
						</xsl:element>
					
						<xsl:element name="mdlist">
							<xsl:attribute name="name" select=" 'modelVersionsYear' "/>
							<xsl:for-each select="tokenize(@modelVersionYear, ',')">
								<xsl:element name="entry">
									<xsl:attribute name="value" select="."/>
								</xsl:element>
							</xsl:for-each>
						</xsl:element>
					</xsl:element>
				</xsl:for-each>
			</xsl:element>
		</xsl:result-document>
		<!-- begin language-specific output -->

		<!-- create Demo.css with fixed content -->
		<xsl:result-document method="text" href="file:///{$baseOutput}{$outputDir}/wsm/css/Demo.css">
			<xsl:value-of select="unparsed-text('mod/demo.css' )" disable-output-escaping="yes"/>
		</xsl:result-document>

		<!-- create webworks.css with fixed content -->
		<xsl:result-document method="text" href="file:///{$baseOutput}{$outputDir}/wsm/css/webworks.css">
			<xsl:value-of select="unparsed-text('mod/webworks.css' )" disable-output-escaping="yes"/>
		</xsl:result-document>

		<!-- create styles.css with fixed content -->
		<xsl:result-document method="text" href="file:///{$baseOutput}{$outputDir}/wsm/css/styles.css">
			<xsl:value-of select="unparsed-text('mod/styles.css' )" disable-output-escaping="yes"/>
		</xsl:result-document>
		
		<!-- create styles_ios.css with fixed content -->
		<xsl:result-document method="text" href="file:///{$baseOutput}{$outputDir}/wsm/css/styles_ios.css">
			<xsl:value-of select="unparsed-text('mod/styles_ios.css' )" disable-output-escaping="yes"/>
		</xsl:result-document>

		<!-- END "helper files" -->

		<!-- output one html file for each topic or task element -->
		<xsl:for-each select="//topic">
			<xsl:variable name="dmsid" select="@vf:dmsid"/>
			<xsl:variable name="topicCode" select="@topicCode"/>
			<xsl:result-document method="xml" href="file:///{$baseOutput}{$outputDir}/wsm/{$lang_dir}/{$topicCode}_{$dmsid}.xml" >
			   <topic>
				<xsl:apply-templates/>
			   </topic>
			</xsl:result-document>
		</xsl:for-each>
	</xsl:template>

	<!--
	<xsl:template match="w3c:pois">
	  <xsl:message>Matched pois in w3c namespace</xsl:message>
	  <pois vf:dmsid="{@vf:dmsid}">
	     <xsl:apply-templates/>
	  </pois>
	</xsl:template>
	-->
	
<!-- copy everything else as is -->
<xsl:template match="@*|node()">
  <xsl:copy>
    <xsl:apply-templates select="@*|node()"/>
  </xsl:copy>
</xsl:template>

</xsl:stylesheet><!-- Stylus Studio meta-information - (c) 2004-2006. Progress Software Corporation. All rights reserved.
<metaInformation>
<scenarios ><scenario default="yes" name="Scenario1" userelativepaths="yes" externalpreview="no" url="..\..\..\..\..\JIRAS\XCDD&#x2D;648\7669&#x2D;20140124\7669&#x2D;20140124\7669.xml" htmlbaseurl="" outputurl="" processortype="internal" useresolver="yes" profilemode="0" profiledepth="" profilelength="" urlprofilexml="" commandline="" additionalpath="" additionalclasspath="" postprocessortype="none" postprocesscommandline="" postprocessadditionalpath="" postprocessgeneratedext="" validateoutput="no" validator="internal" customvalidator=""/></scenarios><MapperMetaTag><MapperInfo srcSchemaPathIsRelative="yes" srcSchemaInterpretAsXML="no" destSchemaPath="" destSchemaRoot="" destSchemaPathIsRelative="yes" destSchemaInterpretAsXML="no"/><MapperBlockPosition></MapperBlockPosition><TemplateContext></TemplateContext><MapperFilter side="source"></MapperFilter></MapperMetaTag>
</metaInformation>
-->