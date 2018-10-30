<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#160;">
	<!ENTITY reg "&#174;">
	<!ENTITY mdash "&#x2014;">
]>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:exsl="http://exslt.org/common" xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse">

	<xsl:param name="user"></xsl:param>
	<xsl:param name="server"></xsl:param>
	<xsl:param name="server_host"></xsl:param>
	<xsl:param name="curlang"></xsl:param>
	<xsl:param name="id"></xsl:param>
	<xsl:param name="ticket"></xsl:param>
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
	
	<xsl:output indent="no" method="html" omit-xml-declaration="no" encoding="UTF-8"/>

	<xsl:template match="topic">
		<html>
					<head>
						<!--meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7"/-->
						<meta http-equiv="Content-Type" content="text/html;charset=utf-8"/>
						<meta http-equiv="Content-Style-Type" content="text/css"/>
						<title>
							<xsl:value-of select="title"/>
						</title>
						
						<script>
							var isMobile = {
								Android: function() {
									return navigator.userAgent.match(/Android/i);
								},
								BlackBerry: function() {
									return navigator.userAgent.match(/BlackBerry/i);
								},
								iOS: function() {
									return navigator.userAgent.match(/iPhone|iPad|iPod/i);
								},
								Opera: function() {
									return navigator.userAgent.match(/Opera Mini/i);
								},
								Windows: function() {
									return navigator.userAgent.match(/Windows/i);
								},
								any: function() {
									return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
								}
							};
			
							if(isMobile.iOS()) {
								document.write('<link rel="StyleSheet" href="../css/styles_ios.css" type="text/css" media="all"/>')
							} else {
								document.write('<link rel="StyleSheet" href="../css/styles.css" type="text/css" media="all"/>')
							}
						</script>
						
						
						<link rel="StyleSheet" href="../css/webworks.css" type="text/css" media="all"/>
						<link rel="StyleSheet" href="../css/Demo.css" type="text/css" media="all"/>
					</head>
					<body>
						<div class="scroll-container">
							<xsl:apply-templates select="body"/>
							<xsl:if test="poitopic|information|animated-procedure[animation-links/tracking-config]">
							  <xsl:call-template name="linkToAR"/>
							</xsl:if>
						</div>
					</body>
				</html>
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
	<xsl:template name="linkToAR">
			<!-- <a href="appurl://xyz?id=">launching the AR App</a> -->
			<!-- arc://<user-info>@<delivery-server>/<ar-resource-type>/<arresource-id>[?<ar-resource-params>][#<ar-resource-fragment>] -->
			
			<!-- ars://[ScenarioType]/[ScenarioId]?provider=[ProviderType]&endpoint=[HostApiUrl]&user=[Username]&token=[SecurityToken] -->
			<!-- arc://dita/WSM_T_1.01.5000_7149?provider=xmlcompass&host=http://192.168.181.83&user=waf3&token=LCQvWldHawRiWVMNDAoACG97fAc=%0D%0A -->
			
			<div>
			<a class="ARlink">
				<xsl:variable name="oScenarioType" select="concat('ars://', 'dita')"/>
				<xsl:variable name="oScenarioId" select="concat('/', $id)"/>
				<xsl:variable name="oDeliveryServer" select="concat('?provider=xmlcompass&#x0026;host=', $server_host)"/>
				<xsl:variable name="oUserInfo" select="concat('&#x0026;user=', $user)"/>
				<xsl:variable name="oTicket" select="concat('&#x0026;token=', $ticket)"/>
				<!--xsl:variable name="oTrackingType" select="concat('&#x0026;trackingtype=', $trackingtype)"/-->
				
				<xsl:attribute name="href">
					<xsl:value-of select="$oScenarioType"/>
					<xsl:value-of select="$oScenarioId"/>
					<xsl:value-of select="$oDeliveryServer"/>
					<xsl:value-of select="$oUserInfo"/>
					<xsl:value-of select="$oTicket"/>
					<!--xsl:value-of select="$oTrackingType"/-->
				</xsl:attribute>
			<img style="height:40px;vertical-align:middle">
				<xsl:attribute name="src">
					<xsl:value-of select="concat($icon_dir, 'btn_ar_active.png')"/>
				</xsl:attribute>
			</img>
			<span><xsl:text>Switch to Augmented Reality!</xsl:text></span>
			</a>
			</div>
			<br/>
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

	<!-- Special handling for SVG graphics -->
	<xsl:template match="image[contains(@href, '.svg')]">
		<embed src="{concat($image_dir,@href)}"  width="80%" type="image/svg+xml" pluginspage="http://www.adobe.com/svg/viewer/install/">
			<!--<xsl:if test="@height != '' ">
				<xsl:attribute name="height" select="@height"/>
			</xsl:if>-->
		</embed>
	</xsl:template>
	
	<!-- Special handling for MP4 video -->
	<xsl:template match="image[contains(@href, '.mp4')]">
		<video src="{concat($image_dir,@href)}"  controls="true"/>
	</xsl:template>
	
	<xsl:template match="video">
		<video controls="true">
		  <source type="video/mp4" src="{concat($image_dir,@href)}"/>
		  <source type="video/ogg" src="{concat($image_dir,concat(substring-before(@href, '.mp4'), '.ogg'))}"/>
		</video>
	</xsl:template>
	
	<xsl:template match="image">
		<img class="Default" width="80%">
		<!--
			<xsl:if test="@width != '' ">
				<xsl:attribute name="width" select="@width"/>
			</xsl:if>
			<xsl:if test="@height != '' ">
				<xsl:attribute name="height" select="@height"/>
			</xsl:if>
			-->
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