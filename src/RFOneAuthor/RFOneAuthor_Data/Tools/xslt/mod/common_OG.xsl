<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:saxon="http://saxon.sf.net/" xmlns:axf="http://www.antennahouse.com/names/XSL/Extensions" xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse" xmlns:atict="http://www.arbortext.com/namespace/atict" exclude-result-prefixes="xlink xs fn xdt" extension-element-prefixes="saxon">
	<!-- VERSION 20120425 -->
	<xsl:variable name="number-format">
		<xsl:text>1</xsl:text>
	</xsl:variable>
	<xsl:template name="SectionName">
		<xsl:param name="family" select="//owners-guide/@family"/>
		<xsl:param name="searchSectNum" select="concat('ogsect_title', @sectionNumber)"/>
		<!--xsl:variable name="boilerFile" select="concat('../lang/boiler_', $language, '.xml')"/>
		<xsl:value-of select="document($boilerFile)//family[@value = $family]/msg[@id = $searchSectNum]"/-->
	</xsl:template>

	<xsl:template match="chapter">
		<xsl:if test="@pgwide='pgwide'">
			<xsl:attribute name="span">all</xsl:attribute>
		</xsl:if>
		<fo:block font-size="9.96pt" id="{@id}" line-height="1em">
			<xsl:call-template name="chapterName"/>
		</fo:block>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template name="chapterName">
		<xsl:param name="chapterNumber" select="substring-after(@chapterCode, '.')"/>
		<xsl:param name="family" select="//owners-guide/@family"/>
		<xsl:param name="searchChaptNum" select="concat('ogchapt-title', $chapterNumber)"/>
		<!--xsl:variable name="boilerFile" select="concat('../lang/boiler_', $language, '.xml')"/>
		<xsl:value-of select="document($boilerFile)//family[@value = $family]/msg[@id = $searchChaptNum]"/-->
	</xsl:template>
	<!--Topic-->
	<xsl:template match="topic | reuse-topic">
		<fo:block id="{@id}">
			<xsl:if test="@break='column'">
				<fo:block break-after="column"/>
			</xsl:if>
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Topic Title-->
	<xsl:template match="owners-guide//topic/title">
		<fo:block font-size="9.94pt" line-height="1em">
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Para-->
	<xsl:template match="para">
		<fo:block xsl:use-attribute-sets="postspace-s">
			<xsl:attribute name="id" select="@id"/>
			<xsl:apply-templates/>
		</fo:block>
	</xsl:template>
	<!--Ptxt-->
	<xsl:template match="ptxt">
		<fo:block>
			<xsl:if test="ancestor::WSM">
				<xsl:attribute name="xsl:use-attribute-sets" select=" 'bodyWSM' "/>
			</xsl:if>
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
			<xsl:choose>
				<xsl:when test="ancestor::WSM">
					<xsl:attribute name="font-size" select=" '11pt' "/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="font-size" select=" '8pt' "/>
				</xsl:otherwise>
			</xsl:choose>
			<fo:float float="left">
				<fo:block>
					<xsl:choose>
						<xsl:when test="ancestor::WSM">
							<xsl:choose>
								<xsl:when test="@type='attention'">
									<fo:external-graphic src="{$icon_path}warning.gif" content-width=".7cm" baseline-shift="12pt"/>
								</xsl:when>
								<xsl:when test="@type='important'">
									<fo:external-graphic src="{$icon_path}important.gif" content-width=".7cm" baseline-shift="12pt"/>
								</xsl:when>
								<xsl:when test="@type='note'">
									<fo:external-graphic src="{$icon_path}note.gif" content-width=".7cm" baseline-shift="12pt"/>
								</xsl:when>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test="@type='attention'">
									<fo:external-graphic src="{$icon_path}warning.gif" content-width="5.98mm" baseline-shift="10pt"/>
								</xsl:when>
								<xsl:when test="@type='important'">
									<fo:external-graphic src="{$icon_path}warning.gif" content-width="5.98mm" baseline-shift="10pt"/>
								</xsl:when>
								<xsl:when test="@type='note'">
									<fo:external-graphic src="{$icon_path}note.gif" content-width="5.98mm" baseline-shift="10pt"/>
								</xsl:when>
							</xsl:choose>
						</xsl:otherwise>
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
			<xsl:choose>
				<xsl:when test="ancestor::WSM">
					<fo:block xsl:use-attribute-sets="bodyWSM">
						<xsl:apply-templates/>
					</fo:block>
				</xsl:when>
				<xsl:otherwise>
					<fo:block xsl:use-attribute-sets="body">
						<xsl:apply-templates/>
					</fo:block>
				</xsl:otherwise>
			</xsl:choose>
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
		<xsl:choose>
			<xsl:when test="ancestor::WSM">
				<fo:block xsl:use-attribute-sets="keep-n prespace bodyWSM postspace">
					<xsl:apply-templates/>
				</fo:block>
			</xsl:when>
			<xsl:otherwise>
				<fo:block xsl:use-attribute-sets="keep-n prespace heading postspace-s">
					<xsl:apply-templates/>
				</fo:block>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!--Steps-->
	<xsl:template match="step">
		<xsl:choose>
			<xsl:when test="ancestor::WSM">
				<xsl:choose>
					<xsl:when test="@num= 'yes'  or not(@num)">
						<fo:list-block xsl:use-attribute-sets="postspace-s keep-tc-col" provisional-distance-between-starts="22pt">
							<fo:list-item>
								<fo:list-item-label end-indent="label-end()">
									<fo:block>
										<xsl:number count="step" from="topic" level="single" format="{$number-format}"/>
										<xsl:text>.</xsl:text>
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
			</xsl:when>
			<xsl:otherwise>
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
				<fo:block break-after="column">
					<fo:leader/>
				</fo:block>
			</xsl:when>
			<xsl:when test="@type = 'page' ">
				<fo:block break-after="page">
					<fo:leader/>
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
		<fo:inline color="blue" text-decoration="underline">
			<fo:basic-link>
				<xsl:attribute name="internal-destination">
					<xsl:value-of select="$xref-link"/>
				</xsl:attribute>
				<xsl:apply-templates/>
				<xsl:text>&#32;</xsl:text>
				<xsl:value-of select="$page.var"/>
				<fo:page-number-citation>
					<xsl:attribute name="ref-id">
						<xsl:value-of select="$xref-link"/>
					</xsl:attribute>
				</fo:page-number-citation>
			</fo:basic-link>
		</fo:inline>
	</xsl:template>
	<xsl:template match="extxref">
		<xsl:variable name="exref" select="@refid"/>
		<xsl:variable name="dms" select="substring-before($exref, '#')"/>
		<xsl:variable name="exref-link" select="substring-after($exref, '#')"/>
		<fo:inline color="blue" text-decoration="underline">
			<fo:basic-link>
				<xsl:attribute name="internal-destination">
					<xsl:value-of select="$exref-link"/>
				</xsl:attribute>
				<xsl:apply-templates/>
				<xsl:text>&#32;</xsl:text>
				<xsl:value-of select="$page.var"/>
				<fo:page-number-citation>
					<xsl:attribute name="ref-id">
						<xsl:value-of select="$exref-link"/>
					</xsl:attribute>
				</fo:page-number-citation>
			</fo:basic-link>
		</fo:inline>
	</xsl:template>
	<xsl:template match="img-ref">
		<xsl:variable name="img-link">
			<xsl:value-of select="@refid"/>
		</xsl:variable>
		<xsl:for-each select="//graphic">
			<xsl:if test="@id = $img-link">
				<fo:inline color="blue" keep-together="always" text-decoration="underline">
					<fo:basic-link>
						<xsl:attribute name="internal-destination">
							<xsl:value-of select="$img-link"/>
						</xsl:attribute>
						<xsl:text>(</xsl:text>
						<fo:page-number-citation>
							<xsl:attribute name="ref-id">
								<xsl:value-of select="$img-link"/>
							</xsl:attribute>
						</fo:page-number-citation>
						<xsl:text>,&#160;</xsl:text>
						<xsl:value-of select="$figure.var"/>
						<xsl:text>&#160;</xsl:text>
						<xsl:number count="figure" format="1" level="any"/>
						<xsl:text>)</xsl:text>
					</fo:basic-link>
				</fo:inline>
			</xsl:if>
		</xsl:for-each>
	</xsl:template>
	<!--Images-->
	<xsl:template match="figure">
		<xsl:variable name="id" select="@id"/>
		<fo:block id="{$id}" xsl:use-attribute-sets="postspace">
			<xsl:attribute name="keep-together.within-column">always</xsl:attribute>
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
			<xsl:number count="figure" level="any"/>
		</xsl:variable>
		<xsl:variable name="id" select="@id"/>
		<xsl:choose>
			<xsl:when test="ancestor::img-block">
				<fo:block clear="both"/>
				<fo:float>
					<xsl:choose>
						<xsl:when test="ancestor::img-block[@page-position = 'left' ]">
							<xsl:attribute name="float" select=" 'left' "/>
						</xsl:when>
						<xsl:when test="ancestor::img-block[@page-position = 'right' ]">
							<xsl:attribute name="float" select=" 'right' "/>
						</xsl:when>
				</xsl:choose>
				<fo:block id="{$id}" xsl:use-attribute-sets="prespace postspace" start-indent="8pt" end-indent="8pt">
					<xsl:if test="ancestor::img-block[@border='yes']">
						<xsl:attribute name="border">.5pt solid black</xsl:attribute>
					</xsl:if>
					<xsl:if test="ancestor::img-block[@pgwide='yes']">
						<xsl:attribute name="span">all</xsl:attribute>
					</xsl:if>
					<fo:external-graphic scaling="uniform">
						<xsl:attribute name="src" select="concat($img_path, @name)"/>
						<xsl:attribute name="width" select="@width"/>
						<xsl:attribute name="content-width" select=" 'scale-to-fit' "/>
						<!--		<xsl:attribute name="height" select="@height"/>-->
						<xsl:attribute name="content-height" select=" 'scale-to-fit' "/>
					</fo:external-graphic>
				</fo:block>
				<xsl:apply-templates select="caption"/>
			</fo:float>
		</xsl:when>
		<xsl:otherwise>
			<fo:block id="{@id}">
				<fo:external-graphic scaling="uniform">
					<xsl:attribute name="src" select="concat($img_path, @name)"/>
					<xsl:attribute name="width" select="@width"/>
					<xsl:attribute name="content-width" select=" 'scale-to-fit' "/>
					<!--		<xsl:attribute name="height" select="@height"/>-->
					<xsl:attribute name="content-height" select=" 'scale-to-fit' "/>
				</fo:external-graphic>
			</fo:block>
			<xsl:if test="not(ancestor::WSM)">
				<fo:block>
					<xsl:value-of select="$figure.var"/>
					<xsl:text>&#160;</xsl:text>
					<xsl:value-of select="$figNr -1"/>
					<xsl:text>.</xsl:text>
					<xsl:apply-templates select="caption"/>
				</fo:block>
			</xsl:if>
		</xsl:otherwise>
	</xsl:choose>
</xsl:template>
<xsl:template match="caption">
	<fo:block xsl:use-attribute-sets="prespace">
		<xsl:apply-templates/>
	</fo:block>
</xsl:template>
<xsl:template match="img-block">
	<xsl:apply-templates/>
	<fo:block clear="both"/>
</xsl:template>
<xsl:template name="tab">
	<fo:inline>&#160;&#160;&#160;&#160;</fo:inline>
</xsl:template>
</xsl:stylesheet>
