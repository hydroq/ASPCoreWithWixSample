<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#160;">
	<!ENTITY reg "&#174;">
	<!ENTITY mdash "&#x2014;">
]>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:exsl="http://exslt.org/common" xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse">
	<!-- VERSION 20120808 -->
	<!-- set this parameter for XMLCompass use or for developer use
	<xsl:param name="baseInput">C:/SPX/Ducati/sampleInput/</xsl:param>-->
	<!-- set this parameter for XMLCompass use or for developer use-->
	<!-- 	<xsl:param name="baseOutput">C:/SPX/Ducati/sampleOutput/test/</xsl:param>-->
	<xsl:param name="baseOutput"></xsl:param>
	<!-- two params come from the command line -->
	<xsl:param name="outputDir"></xsl:param>
	<xsl:param name="curlang"></xsl:param>
	<!-- these two directories may need to be localized -->
	<xsl:param name="icon_dir" select=" 'img/' "/>
	<xsl:param name="image_dir" select=" 'img/' "/>
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
	<xsl:include href="mod/cals2html.xsl"/>
	<xsl:include href="mod/extxref_wsm.xsl"/>
	<xsl:include href="lang/lang.xss"/>
	<xsl:template match="WSM">
		<xsl:param name="family" select="@model"/>
		<xsl:param name="outputDir" select="$outputDir"/>
		<xsl:param name="outputName" select="concat('info_', $curlang, '.xml')"/>
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
							<xsl:attribute name="name" select=" 'SectionCode' "/>
							<xsl:attribute name="value" select="@vf:SectionCode"/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'ChapterCode' "/>
							<xsl:attribute name="value" select="@vf:ChapterCode"/>
						</xsl:element>
						<xsl:element name="md">
							<xsl:attribute name="name" select=" 'TopicCode' "/>
							<xsl:attribute name="value" select="@vf:TopicCode"/>
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
					</xsl:element>
					<xsl:element name="mdlist">
						<xsl:attribute name="name" select=" 'model' "/>
						<xsl:for-each select="tokenize(@vf:model, ',')">
							<xsl:element name="entry">
								<xsl:attribute name="value" select="."/>
							</xsl:element>
						</xsl:for-each>
					</xsl:element>
					<xsl:element name="mdlist">
						<xsl:attribute name="name" select=" 'ModelYear' "/>
						<xsl:for-each select="tokenize(@vf:modelYear, ',')">
							<xsl:element name="entry">
								<xsl:attribute name="value" select="."/>
							</xsl:element>
						</xsl:for-each>
					</xsl:element>
				</xsl:for-each>
			</xsl:element>
		</xsl:result-document>
		<!-- begin language-specific output -->
		<!-- create Ducati.css with fixed content -->
		<xsl:result-document method="text" href="file:///{$baseOutput}{$outputDir}/wsm/css/Ducati.css">
			<xsl:value-of select="unparsed-text('mod/Ducati.css' )" disable-output-escaping="yes"/>
		</xsl:result-document>
		<!-- create webworks.css with fixed content -->
		<xsl:result-document method="text" href="file:///{$baseOutput}{$outputDir}/wsm/css/webworks.css">
			<xsl:value-of select="unparsed-text('mod/webworks.css' )" disable-output-escaping="yes"/>
		</xsl:result-document>
		<!-- create styles.css with fixed content -->
		<xsl:result-document method="text" href="file:///{$baseOutput}{$outputDir}/wsm/css/styles.css">
			<xsl:value-of select="unparsed-text('mod/styles.css' )" disable-output-escaping="yes"/>
		</xsl:result-document>
		<!-- END "helper files" -->
		<!-- output one html file for each task or topic element -->
		<xsl:for-each select="//topic">
			<xsl:variable name="dmsid" select="@vf:dmsid"/>
			<xsl:variable name="filepath" select="concat($baseOutput, $outputDir, '/wsm/', $lang_dir, '/', $dmsid, '.html')"/>
			<xsl:message>
			    <!--<xsl:variable xmlns:java="http://saxon.sf.net/java-type" name="output" select="java:System.out.println('zzzzzzzzzzzzzzzzzz')"/>-->
<!--			    <xsl:variable name="output" select="out:println()" xmlns:system="java:java.lang.System" xmlns:out="system:out()"/>
			    <xsl:value-of select="output" />-->
			    <xsl:value-of select="trace(concat('PUBLICATIONOUTPUT ', $filepath),'dummy')"/>
			</xsl:message>

			<xsl:result-document method="html" href="file:///{$baseOutput}{$outputDir}/wsm/{$lang_dir}/{$dmsid}.html">
				<html>
					<head>
						<meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7"/>
						<meta http-equiv="Content-Type" content="text/html;charset=utf-8"/>
						<meta http-equiv="Content-Style-Type" content="text/css"/>
						<title>
							<xsl:value-of select="title"/>
						</title>
						<link rel="StyleSheet" href="../css/styles.css" type="text/css" media="all"/>
						<link rel="StyleSheet" href="../css/webworks.css" type="text/css" media="all"/>
						<link rel="StyleSheet" href="../css/Ducati.css" type="text/css" media="all"/>
					</head>
					<body>
						<xsl:apply-templates/>
					</body>
				</html>
			</xsl:result-document>
		</xsl:for-each>
	</xsl:template>
	<xsl:template name="nameValue">
		<xsl:param name="searchCode"/>
		<xsl:param name="family"/>
		<!--xsl:param name="boilerFile"/>
                <xsl:attribute name="value">
                <xsl:value-of select="document($boilerFile)//family[@value = $family]/msg[@id = $searchCode]"/>
        </xsl:attribute-->
	</xsl:template>
	<xsl:template match="topic/title | task/title">
		<div class="Paragrafo_outer" style="margin-left: 0pt;margin-top: 5pt;">
			<table border="0" cellspacing="0" cellpadding="0" id="{@id}">
				<tr style="vertical-align: baseline;">
					<td>
						<div class="Paragrafo_inner" style="width: 18.4251968503937pt; white-space: nowrap;">
							<xsl:if test="parent::task">
								<xsl:number count="task" level="single"/>
							</xsl:if>
							<xsl:if test="parent::topic">
								<xsl:number count="topic" level="single"/>
							</xsl:if>-</div>
					</td>
					<td width="100%">
						<div class="Paragrafo_inner">
							<a>
								<xsl:attribute name="name" select="ancestor::topic/@id"/>
							</a>
							<xsl:apply-templates/>
						</div>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>
	<xsl:template match="reuse-topic/title | table/title">
		<div class="SOTTO_PARA">
			<a>
				<xsl:attribute name="name" select="@id"/>
			</a>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="p">
		<div class="SOTTO_PARA">
			<a>
				<xsl:attribute name="name" select="generate-id()"/>
				<xsl:text> </xsl:text>
			</a>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="p/title | table/title">
		<div class="titoleto">
			<a>
				<xsl:attribute name="name" select="generate-id()"/>
				<xsl:text> </xsl:text>
			</a>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="fig/title | figure-2d/title">
		<div class="titoleto">
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="title">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="emph">
		<span class="bold">
			<xsl:apply-templates/>
		</span>
	</xsl:template>
	<xsl:template match="fig | figure-2d">
		<div class="AggancioFigure">
			<center>
				<xsl:apply-templates/>
			</center>
		</div>
	</xsl:template>
	<xsl:template match="animated-procedure">
		<xsl:for-each select="media-3d">
			<xsl:variable name="oName" select="concat(@vf:dmsidref, '.html')"/>
			<IFRAME SRC="{$oName}" width="100%" height="600px">
			</IFRAME>
		</xsl:for-each>
	</xsl:template>
	<xsl:template match="caption">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="note">
		<div class="nota">
			<img class="nota" width="28" height="25" style="display: inline; float: none; left: 0.0; text-align: left; top: 0.0;">
				<xsl:attribute name="src"><xsl:value-of select="concat($icon_dir, 'note.jpg')"/></xsl:attribute>
			</img>
			<xsl:value-of select="$note.title"/>
		</div>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="attention">
		<div class="Attenzione">
			<img class="Attenzione" width="28" height="25" style="display: inline; float: none; left: 0.0; text-align: left; top: 0.0;">
				<xsl:attribute name="src"><xsl:value-of select="concat($icon_dir, 'warning.jpg' )"/></xsl:attribute>
			</img>
			<xsl:value-of select="$attention.title"/>
		</div>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="warning">
		<div class="Attenzione">
			<img class="Attenzione" width="28" height="25" style="display: inline; float: none; left: 0.0; text-align: left; top: 0.0;">
				<xsl:attribute name="src"><xsl:value-of select="concat($icon_dir, 'warning.jpg' )"/></xsl:attribute>
			</img>
			<xsl:value-of select="$warning.title"/>
		</div>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="important">
		<div class="Attenzione">
			<img class="Attenzione" width="28" height="25" style="display: inline; float: none; left: 0.0; text-align: left; top: 0.0;">
				<xsl:attribute name="src"><xsl:value-of select="concat($icon_dir, 'important.jpg' )"/></xsl:attribute>
			</img>
			<xsl:value-of select="$important.title"/>
		</div>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="callout">
		<div class="Trattino_outer" style="margin-left: 0pt;">
			<xsl:choose>
				<xsl:when test="pos">
					<xsl:for-each select="pos">
						<table border="0" cellspacing="0" cellpadding="0">
							<tr style="vertical-align: baseline;">
								<td>
									<div class="Trattino_inner" style="width: 24pt; white-space: nowrap;">
										<xsl:value-of select="label"/>
									</div>
								</td>
								<td width="100%">
									<div class="Trattino_inner">
										<xsl:apply-templates select="para"/>
									</div>
								</td>
							</tr>
						</table>
					</xsl:for-each>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates/>
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>
	<xsl:template match="xref | intxref">
		<xsl:element name="a">
			<xsl:attribute name="href" select="concat('#', @refid)"/>
			<xsl:apply-templates/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="ol | ul">
		<div class="SOTTO_PARA">
			<xsl:choose>
				<xsl:when test="ancestor::li">
					<xsl:attribute name="class" select=" 'SOTTO_PARA_inner' "/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="class" select=" 'SOTTO_PARA' "/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="li">
		<div class="Trattino_outer" style="margin-left: 0pt;">
			<table border="0" cellspacing="0" cellpadding="0">
				<tr style="vertical-align: baseline;">
					<td>
						<xsl:choose>
							<xsl:when test="ancestor::ul[@type = 'bullet']">
								<div class="Trattino_inner" style="width: 20pt; white-space: nowrap;">&#x2022;</div>
							</xsl:when>
							<xsl:when test="ancestor::ul[@type = 'dash']">
								<div class="Trattino_inner" style="width: 20pt; white-space: nowrap;">-</div>
							</xsl:when>
							<xsl:when test="ancestor::ol[@type = 'arabicnum'] | ancestor::ol">
								<div class="Trattino_inner" style="width: 20pt; white-space: nowrap;">
									<xsl:number count="li" from="ol"/>
									<xsl:text> </xsl:text>
								</div>
							</xsl:when>
							<xsl:otherwise>
								<div class="Trattino_inner" style="width: 20pt; white-space: nowrap;">&#x2022;</div>
							</xsl:otherwise>
						</xsl:choose>
					</td>
					<td width="100%">
						<div class="Trattino_inner">
							<xsl:apply-templates/>
						</div>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="graphic-2d">
		<EMBED class="Default">
			<xsl:if test="@width != '' ">
				<xsl:attribute name="width" select="@width"/>
			</xsl:if>
			<xsl:if test="@height != '' ">
				<xsl:attribute name="height" select="@height"/>
			</xsl:if>

			<!--			<xsl:if test="@placement='inline'">
				<xsl:attribute name="width">28</xsl:attribute>
				<xsl:attribute name="height">25</xsl:attribute>
				<xsl:attribute name="style">display: inline; float: none; left: 0.0; text-align: left; top: 0.0;</xsl:attribute>
			</xsl:if>-->
			<xsl:attribute name="src" select="concat($image_dir, @href)"/>
			<xsl:attribute name="TYPE" select="concat('image/',substring-after(@href,'.'))"/>
		</EMBED>
	</xsl:template>

	<xsl:template match="image">
		<img class="Default">
			<xsl:if test="@width != '' ">
				<xsl:attribute name="width" select="@width"/>
			</xsl:if>
			<xsl:if test="@height != '' ">
				<xsl:attribute name="height" select="@height"/>
			</xsl:if>
			<!--			<xsl:if test="@placement='inline'">
				<xsl:attribute name="width">28</xsl:attribute>
				<xsl:attribute name="height">25</xsl:attribute>
				<xsl:attribute name="style">display: inline; float: none; left: 0.0; text-align: left; top: 0.0;</xsl:attribute>
			</xsl:if>-->
			<xsl:attribute name="src" select="concat($image_dir, @href)"/>
		</img>
	</xsl:template>
	<xsl:template match="simpletable">
		<table border="1" cellspacing="0" cellpadding="0">
			<xsl:apply-templates/>
		</table>
	</xsl:template>
	<xsl:template match="sthead">
		<xsl:for-each select="stentry">
			<th>
				<xsl:apply-templates/>
			</th>
		</xsl:for-each>
	</xsl:template>
	<xsl:template match="strow">
		<tr style="vertical-align: baseline;">
			<xsl:for-each select="stentry">
				<td>
					<div class="Trattino_inner">
						<xsl:apply-templates/>
					</div>
				</td>
			</xsl:for-each>
		</tr>
	</xsl:template>
	<xsl:template match="stepTEST">
		<xsl:choose>
			<xsl:when test="@num = 'yes' ">
				<div class="SOTTO_PARA">
					<a name="{@id}"/>
					<table border="0" cellspacing="0" cellpadding="0" id="{@id}">
						<tr style="vertical-align: baseline;">
							<td>
								<div class="Trattino_inner" style="width: 12pt; white-space: nowrap;">
									<xsl:number count="step1" level="single"/>
								</div>
							</td>
							<td width="100%">
								<div class="Paragrafo_inner">
									<a>
										<xsl:attribute name="name" select="generate-id()"/>
										<xsl:apply-templates select="ptxt"/>
									</a>
								</div>
							</td>
						</tr>
					</table>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<div class="SOTTO_PARA">
					<xsl:apply-templates/>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="special_tool">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="Codenumber">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="no_translate">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="taskbody">
		<div class="SOTTO_PARA">
			<a>
				<xsl:attribute name="name" select="generate-id()"/>
				<xsl:text> </xsl:text>
			</a>
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="steps">
		<div class="SOTTO_PARA">
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="substeps">
		<div class="SOTTO_PARA">
			<xsl:apply-templates/>
		</div>
	</xsl:template>
	<xsl:template match="step">
		<div class="Trattino_outer" style="margin-left: 0pt;">
			<table border="0" cellspacing="0" cellpadding="0">
				<tr style="vertical-align: baseline;">
					<td>
						<div class="Trattino_inner" style="width: 20pt; white-space: nowrap;">
							<xsl:number count="step" from="steps" format="1."/>
							<xsl:text> </xsl:text>
						</div>
					</td>
					<td width="100%">
						<div class="Trattino_inner">
							<xsl:apply-templates/>
						</div>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>
	<xsl:template match="substep">
		<div class="Trattino_outer" style="margin-left: 0pt;">
			<table border="0" cellspacing="0" cellpadding="0">
				<tr style="vertical-align: baseline;">
					<td>
						<div class="Trattino_inner" style="width: 20pt; white-space: nowrap;">
							<xsl:number count="substep" from="substeps" format="A."/>
							<xsl:text> </xsl:text>
						</div>
					</td>
					<td width="100%">
						<div class="Trattino_inner">
							<xsl:apply-templates/>
						</div>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>
	<xsl:template match="cmd | result">
		<xsl:apply-templates/>
	</xsl:template>
</xsl:stylesheet>

<!-- Stylus Studio meta-information - (c) 2004-2006. Progress Software Corporation. All rights reserved.
<metaInformation>
<scenarios/><MapperMetaTag><MapperInfo srcSchemaPathIsRelative="yes" srcSchemaInterpretAsXML="no" destSchemaPath="" destSchemaRoot="" destSchemaPathIsRelative="yes" destSchemaInterpretAsXML="no"/><MapperBlockPosition></MapperBlockPosition><TemplateContext></TemplateContext><MapperFilter side="source"></MapperFilter></MapperMetaTag>
</metaInformation>
-->