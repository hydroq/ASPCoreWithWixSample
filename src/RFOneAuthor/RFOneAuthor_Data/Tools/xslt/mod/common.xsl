<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:saxon="http://saxon.sf.net/" xmlns:axf="http://www.antennahouse.com/names/XSL/Extensions" xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse" xmlns:atict="http://www.arbortext.com/namespace/atict" exclude-result-prefixes="xlink xs fn xdt" extension-element-prefixes="saxon">
	<xsl:variable name="number-format">
		<xsl:text>1</xsl:text>
	</xsl:variable>
	<xsl:template name="SectionName">
	<xsl:value-of select="@sectionName"/>
	</xsl:template>
	<xsl:template match="chapter">
		<xsl:variable name="chapId" select="concat('dms', @vf:dmsid)"/>
		<fo:block font-size="9.96pt" id="{$chapId}">
			<xsl:if test="@pgwide='yes'">
				<xsl:attribute name="span">all</xsl:attribute>
			</xsl:if>
			<xsl:if test="@break='page'">
				<fo:block break-before="page"/>
			</xsl:if>
			<xsl:if test="@break='column'">
				<fo:block break-before="column"/>
			</xsl:if>
			<xsl:call-template name="chapterName"/>
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<xsl:template name="chapterName">
	<xsl:value-of select="@chapterName"/>
	</xsl:template>
	<!--Topic-->
	<xsl:template match="topic | reuse-topic">
		<fo:block id="{@id}">
			<xsl:if test="@pgwide='yes'">
				<xsl:attribute name="span">all</xsl:attribute>
			</xsl:if>
			<xsl:if test="@break='column'">
				<fo:block break-after="column"/>
			</xsl:if>
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Topic Title-->
	<xsl:template match="owners-guide//topic/title">
		<fo:block font-size="9.94pt">
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Para-->
	<xsl:template match="para">
		<fo:block xsl:use-attribute-sets="postspace-s" font-size="8pt">
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Ptxt-->
	<xsl:template match="ptxt">
		<fo:block font-size="8pt">
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Superscript-->
	<xsl:template match="sup">
		<fo:inline font-size="6pt" vertical-align="super">
			<xsl:apply-templates/>
		</fo:inline>
	</xsl:template>
	<!--Subscript-->
	<xsl:template match="sub">
		<fo:inline font-size="6pt" vertical-align="sub">
			<xsl:apply-templates/>
		</fo:inline>
	</xsl:template>
	<!--Emphasis and its different Types-->
	<xsl:template match="emph">
		<fo:inline>
			<xsl:choose>
				<xsl:when test="not(@eType) or @eType='bold'">
					<xsl:attribute name="font-weight">bold</xsl:attribute>
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:when test="@eType='italic'">
					<xsl:attribute name="font-style">italic</xsl:attribute>
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:when test="@eType='bolditalic'">
					<xsl:attribute name="font-style">italic</xsl:attribute>
					<xsl:attribute name="font-weight">bold</xsl:attribute>
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:when test="@eType='underline'">
					<xsl:attribute name="text-decoration">underline</xsl:attribute>
					<xsl:apply-templates/>
				</xsl:when>
				<xsl:when test="@eType='normal'">
					<xsl:attribute name="font-style">normal</xsl:attribute>
					<xsl:attribute name="font-weight">normal</xsl:attribute>
					<xsl:apply-templates/>
				</xsl:when>
			</xsl:choose>
		</fo:inline>
	</xsl:template>
	<!--Attention ('Important', 'Attention' and 'Note')-->
	<xsl:template match="attention">
		<fo:block keep-together.within-column="always" xsl:use-attribute-sets="postspace prespace" id="{@id}">
			<xsl:attribute name="font-size" select=" '10pt' "/>
			<fo:float float="left">
				<fo:block>
					<xsl:choose>
						<xsl:when test="@type='attention'">
							<fo:external-graphic src="{$icon_path}attention.eps" content-width="5.98mm" baseline-shift="-10pt"/>
						</xsl:when>
						<xsl:when test="@type='important'">
							<fo:external-graphic src="{$icon_path}attention.eps" content-width="5.98mm" baseline-shift="-10pt"/>
						</xsl:when>
						<xsl:when test="@type='note'">
							<fo:external-graphic src="{$icon_path}note.eps" content-width="5.98mm" baseline-shift="-10pt"/>
						</xsl:when>
					</xsl:choose>
					<fo:leader leader-pattern="space" leader-length="2pt"/>
				</fo:block>
			</fo:float>
			<xsl:choose>
				<xsl:when test="@type='important'">
					<xsl:value-of select="$important.var"/>
				</xsl:when>
				<xsl:when test="@type='attention'">
					<xsl:value-of select="$attention.var"/>
				</xsl:when>
				<xsl:when test="@type='note'">
					<xsl:value-of select="$note.var"/>
				</xsl:when>
			</xsl:choose>
			<fo:block font-size="8pt">
				<xsl:apply-templates/>
			</fo:block>
		</fo:block>
	</xsl:template>
	<!-- List-->
	<xsl:template match="list">
		<fo:block>
			<xsl:apply-templates select="title"/>
			<fo:list-block xsl:use-attribute-sets="postspace" provisional-label-separation="4pt">
				<xsl:choose>
					<xsl:when test="(@type = 'arabicnum' or not(@type)) and count(descendant::item) &gt;  9">
						<xsl:attribute name="provisional-distance-between-starts" select=" '20pt' "/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="provisional-distance-between-starts" select=" '15pt' "/>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:apply-templates select="item"/>
			</fo:list-block>
		</fo:block>
	</xsl:template>
	<xsl:template match="item">
		<fo:list-item xsl:use-attribute-sets="keep-tc-col">
			<fo:list-item-label end-indent="label-end()">
				<fo:block>
					<xsl:choose>
						<xsl:when test="../@type = 'bullet'">
							<xsl:attribute name="font-family" select=" 'Arial' "/>
							<xsl:text>&#x25CF;</xsl:text>
						</xsl:when>
						<xsl:when test="../@type = 'dash'">-</xsl:when>
						<xsl:when test="../@type = 'arabicnum'">
							<xsl:number format="1"/>
							<xsl:text>)</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:number format="1"/>
							<xsl:text>)</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</fo:block>
			</fo:list-item-label>
			<fo:list-item-body start-indent="body-start()">
				<fo:block>
					<xsl:apply-templates/>
				</fo:block>
			</fo:list-item-body>
		</fo:list-item>
	</xsl:template>
	<!--Para and List Titles-->
	<xsl:template match="para/title | list/title">
		<fo:block xsl:use-attribute-sets="heading postspace-s">
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Steps-->
	<xsl:template match="step">
		<xsl:choose>
			<xsl:when test="@num= 'yes' ">
				<fo:list-block xsl:use-attribute-sets="postspace-s keep-tc-col" provisional-distance-between-starts="18pt">
					<fo:list-item>
						<fo:list-item-label end-indent="label-end()">
							<fo:block>
								<xsl:number count="step" from="topic" level="single" format="{$number-format}"/>
							</fo:block>
						</fo:list-item-label>
						<fo:list-item-body start-indent="body-start()">
							<xsl:apply-templates/>
						</fo:list-item-body>
					</fo:list-item>
				</fo:list-block>
			</xsl:when>
			<xsl:otherwise>
				<fo:block xsl:use-attribute-sets="postspace-s keep-tc-col">
					<xsl:apply-templates/>
				</fo:block>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!--Callout-->
	<xsl:template match="callout">
		<fo:list-block xsl:use-attribute-sets="keep-p3" provisional-distance-between-starts="18pt">
			<xsl:if test="ancestor::WSM">
				<xsl:attribute name="margin-left" select=" '3em' "/>
			</xsl:if>
			<xsl:apply-templates/>
		</fo:list-block>
	</xsl:template>
	<!--Callout Position-->
	<xsl:template match="callout/pos">
		<fo:list-item xsl:use-attribute-sets="postspace-s">
			<fo:list-item-label end-indent="label-end()">
				<fo:block>
					<xsl:choose>
						<xsl:when test="label">
							<xsl:apply-templates select="label"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:number count="pos" from="callout" format="1)"/>
						</xsl:otherwise>
					</xsl:choose>
				</fo:block>
			</fo:list-item-label>
			<fo:list-item-body start-indent="body-start()" xsl:use-attribute-sets="keep-tc-col">
				<xsl:apply-templates select="para | ptxt"/>
			</fo:list-item-body>
		</fo:list-item>
	</xsl:template>
	<!--Label-->
	<xsl:template match="label">
		<xsl:apply-templates/>
	</xsl:template>
	<!--Callout Paragraph-->
	<xsl:template match="callout/para">
		<fo:list-item xsl:use-attribute-sets="postspace-s">
			<fo:list-item-label>
				<fo:block>
					<fo:leader leader-pattern="space"/>
				</fo:block>
			</fo:list-item-label>
			<fo:list-item-body>
				<fo:block>
					<xsl:apply-templates/>
				</fo:block>
			</fo:list-item-body>
		</fo:list-item>
	</xsl:template>
	<!-- Footnote-->
	<xsl:template match="ftnote">
		<fo:footnote axf:suppress-duplicate-footnote="true">
			<fo:inline baseline-shift="super" font-size="75%">
				<xsl:number count="ftnote" format="1" level="any"/>
			</fo:inline>
			<fo:footnote-body start-indent="0pt" text-align="left" text-indent="0pt">
				<fo:list-block provisional-distance-between-starts="12pt" provisional-label-separation="2pt" text-align="left">
					<fo:list-item>
						<fo:list-item-label end-indent="label-end()">
							<fo:block keep-together.within-line="6">
								<fo:inline baseline-shift="super" font-size="75%">
									<xsl:number count="ftnote" format="1" level="any"/>
									<xsl:text>) </xsl:text>
								</fo:inline>
							</fo:block>
						</fo:list-item-label>
						<fo:list-item-body start-indent="body-start()">
							<fo:block>
								<xsl:apply-templates/>
							</fo:block>
						</fo:list-item-body>
					</fo:list-item>
				</fo:list-block>
			</fo:footnote-body>
		</fo:footnote>
	</xsl:template>
	<!-- Break and its type-->
	<xsl:template match="br">
		<xsl:choose>
			<xsl:when test="@type = 'line' ">
				<fo:block height="1pt"/>
			</xsl:when>
			<xsl:when test="@type = 'column' ">
				<fo:block break-before="column">
				</fo:block>
			</xsl:when>
			<xsl:when test="@type = 'page' ">
				<fo:block break-before="page">
				</fo:block>
			</xsl:when>
			<xsl:otherwise>
				<fo:block height="1pt"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!--No Break-->
	<xsl:template match="nobreak">
		<fo:inline xsl:use-attribute-sets="keep-line">
			<xsl:apply-templates/>
		</fo:inline>
	</xsl:template>
	<!--Symbol-->
	<xsl:template match="symbol">
		<xsl:variable name="strWidth">
			<xsl:choose>
				<xsl:when test="@width">
					<xsl:value-of select="@width"/>
				</xsl:when>
				<xsl:otherwise>4mm</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="strHeight">
			<xsl:choose>
				<xsl:when test="@height">
					<xsl:value-of select="@height"/>
				</xsl:when>
				<xsl:otherwise>3.1mm</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="parent::*[normalize-space()]">
				<fo:inline>
					<xsl:text>&#160;</xsl:text>
					<fo:external-graphic src="{$img_path}{@name}" content-width="{$strWidth}"/>
					<xsl:text>&#160;</xsl:text>
				</fo:inline>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>&#160;</xsl:text>
				<fo:external-graphic src="{$img_path}{@name}" content-width="{$strWidth}"/>
				<xsl:text>&#160;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="icon">
		<xsl:variable name="strWidth">
			<xsl:choose>
				<xsl:when test="@width">
					<xsl:value-of select="@width"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="strHeight">
			<xsl:choose>
				<xsl:when test="@height">
					<xsl:value-of select="@height"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="parent::*[normalize-space()]">
				<fo:inline>
					<xsl:text>&#160;</xsl:text>
					<fo:external-graphic src="{$img_path}{@name}" content-width="{$strWidth}"/>
					<xsl:text>&#160;</xsl:text>
				</fo:inline>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>&#160;</xsl:text>
				<fo:external-graphic src="{$img_path}{@name}" content-width="{$strWidth}"/>
				<xsl:text>&#160;</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!--Internal Reference-->
	<xsl:template match="intxref">
		<xsl:variable name="xref-link" select="@refid"/>
		<fo:inline>
			<fo:basic-link>
				<xsl:attribute name="internal-destination"><xsl:value-of select="$xref-link"/></xsl:attribute>
				<xsl:apply-templates/>
				<xsl:value-of select="$page.var"/>
				<fo:page-number-citation>
					<xsl:attribute name="ref-id"><xsl:value-of select="$xref-link"/></xsl:attribute>
				</fo:page-number-citation>
			</fo:basic-link>
		</fo:inline>
	</xsl:template>
	<xsl:template match="extxref">
		<xsl:variable name="exref" select="@refid"/>
		<xsl:variable name="dms" select="substring-before($exref, '#')"/>
		<xsl:variable name="exref-link" select="substring-after($exref, '#')"/>
		<fo:inline>
			<fo:basic-link>
				<xsl:attribute name="internal-destination"><xsl:value-of select="$exref-link"/></xsl:attribute>
				<xsl:apply-templates/>
				<xsl:value-of select="$page.var"/>
				<fo:page-number-citation>
					<xsl:attribute name="ref-id"><xsl:value-of select="$exref-link"/></xsl:attribute>
				</fo:page-number-citation>
			</fo:basic-link>
		</fo:inline>
	</xsl:template>
	<xsl:template match="img-ref">
		<xsl:variable name="img-link">
			<xsl:value-of select="@refid"/>
		</xsl:variable>
		<fo:inline keep-together="always">
			<fo:basic-link>
				<xsl:attribute name="internal-destination"><xsl:value-of select="$img-link"/></xsl:attribute>
				<xsl:text>(</xsl:text>
				<xsl:apply-templates/>
				<xsl:for-each select="//graphic">
					<xsl:if test="@id = $img-link">
						<xsl:variable name="figNr">
							<xsl:number count="graphic" level="any"/>
						</xsl:variable>
						<xsl:value-of select="$figure.var"/>
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="$figNr -1"/>
						<xsl:text>)</xsl:text>
					</xsl:if>
				</xsl:for-each>
			</fo:basic-link>
		</fo:inline>
	</xsl:template>
	<xsl:template match="img-ref-external">
		<xsl:variable name="img-link">
			<xsl:value-of select="@refid"/>
		</xsl:variable>
		<xsl:variable name="dms" select="substring-before($img-link, '#')"/>
		<xsl:variable name="exref-link" select="substring-after($img-link, '#')"/>
		<fo:inline keep-together="always">
			<fo:basic-link>
				<xsl:attribute name="internal-destination"><xsl:value-of select="$exref-link"/></xsl:attribute>
				<xsl:text>(</xsl:text>
				<xsl:apply-templates/>
				<xsl:for-each select="//graphic">
					<xsl:if test="@id = $exref-link">
						<xsl:variable name="figNr">
							<xsl:number count="graphic" level="any"/>
						</xsl:variable>
						<xsl:value-of select="$figure.var"/>
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="$figNr -1"/>
						<xsl:text>)</xsl:text>
					</xsl:if>
				</xsl:for-each>
			</fo:basic-link>
		</fo:inline>
	</xsl:template>
	<!--Images-->
	<xsl:template match="figure">
		<xsl:variable name="id" select="@id"/>
		<fo:block id="{$id}" xsl:use-attribute-sets="postspace">
			<xsl:apply-templates select="graphic"/>
			<xsl:apply-templates select="title"/>
			<xsl:apply-templates select="callout"/>
		</fo:block>
	</xsl:template>
	<xsl:template match="figure/title">
		<fo:block font-size="9pt" xsl:use-attribute-sets="postspace">
			<xsl:attribute name="id" select="@id"/>
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<xsl:template match="graphic">
		<xsl:variable name="figNr">
			<xsl:number count="graphic" level="any"/>
		</xsl:variable>
		<fo:block id="{@id}" text-align="center">
			<xsl:if test="ancestor::img-block[@pgwide='yes']">
				<xsl:attribute name="span">all</xsl:attribute>
			</xsl:if>
			<xsl:if test="ancestor::img-block[@pageposition='bottom']">
				<xsl:attribute name="span">all</xsl:attribute>
			</xsl:if>
			<fo:external-graphic>
				<xsl:attribute name="src" select="concat($img_path, @name)"/>
				<xsl:attribute name="content-height"><xsl:value-of select="@height"/></xsl:attribute>
				<xsl:attribute name="content-width"><xsl:value-of select="@width"/></xsl:attribute>
			</fo:external-graphic>
		</fo:block>
		<fo:block text-align="right" margin-right="1mm" font-size="8pt">
			<xsl:value-of select="$figure.var"/>
			<xsl:text>&#160;</xsl:text>
			<xsl:value-of select="$figNr -1"/>
		</fo:block>
	</xsl:template>
	<xsl:template match="img-block">
		<fo:block-container space-after="2mm" keep-together="always">
			<xsl:choose>
				<xsl:when test="@border='yes'">
					<xsl:attribute name="border">.5pt solid black</xsl:attribute>
				</xsl:when>
			</xsl:choose>
			<fo:block>
				<xsl:apply-templates/>
			</fo:block>
		</fo:block-container>
	</xsl:template>
	<xsl:template match="image-text">
		<fo:block-container left="68mm" keep-together="always">
			<fo:block><xsl:apply-templates/></fo:block>
		</fo:block-container>
	</xsl:template>
	<xsl:template name="staticText">
		<fo:block break-after="page">
			<xsl:choose>
				<xsl:when test="$family='M'">
					<xsl:apply-templates select="owners-guide/section[@sectionNumber='M.000']"/>
				</xsl:when>
				<xsl:when test="$family='D'">
					<xsl:apply-templates select="owners-guide/section[@sectionNumber='D.000']"/>
				</xsl:when>
				<xsl:when test="$family='SBK'">
					<xsl:apply-templates select="owners-guide/section[@sectionNumber='SBK.000']"/>
				</xsl:when>
				<xsl:when test="$family='F'">
					<xsl:apply-templates select="owners-guide/section[@sectionNumber='F.000']"/>
				</xsl:when>
				<xsl:when test="$family='HYM'">
					<xsl:apply-templates select="owners-guide/section[@sectionNumber='HYM.000']"/>
				</xsl:when>
				<xsl:when test="$family='MTS'">
					<xsl:apply-templates select="owners-guide/section[@sectionNumber='MTS.000']"/>
				</xsl:when>
			</xsl:choose>
		</fo:block>
	</xsl:template>
	<xsl:template name="tab">
		<fo:inline>&#160;&#160;&#160;&#160;</fo:inline>
	</xsl:template>
	<xsl:template match="owners-guide/section[@sectionName='Libretto di uso e manutenzione']//figure[1]"/>
</xsl:stylesheet>
