<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:saxon="http://saxon.sf.net/" xmlns:axf="http://www.antennahouse.com/names/XSL/Extensions" xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse" exclude-result-prefixes="xlink xs fn xdt" extension-element-prefixes="saxon">
	<!-- ************************ TOC *********************** -->
	<xsl:template name="TOCpage">
		<fo:block xsl:use-attribute-sets="TOC" id="TOC" space-after="22mm">
			<xsl:value-of select="$toc.var"/>
		</fo:block>
		<fo:block>
			<fo:block>
				<xsl:apply-templates select="owners-guide" mode="build-toc"/>
			</fo:block>
		</fo:block>
		<!--<fo:block break-after="page"/>-->
	</xsl:template>
	<xsl:template match="owners-guide" mode="build-toc">
		<xsl:for-each select="section">
			<xsl:sort select="@sectionNumber" data-type="number" order="ascending"/>
			<xsl:choose>
				<xsl:when test="@sectionName='Libretto di uso e manutenzione'"/>
				<xsl:otherwise>
					<fo:block font-size="9.96pt" space-after="5.62mm">
						<xsl:variable name="id" select="@id"/>
						<xsl:call-template name="SectionName"/>
						<xsl:text>&#160;&#160;&#160;&#160;</xsl:text>
						<fo:page-number-citation ref-id="{$id}"/>
						<xsl:apply-templates select="chapter" mode="build-toc"/>
					</fo:block>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:for-each>
		<!-- <fo:block break-after="page"/> -->
		<xsl:call-template name="section"/>
	</xsl:template>
	<xsl:template match="chapter" mode="build-toc">
		<fo:block font-size="8pt">
			<xsl:variable name="ChapterId" select="topic[1]/@id"/>
			<xsl:call-template name="chapterName"/>
			<xsl:call-template name="tab"/>
			<fo:page-number-citation ref-id="{$ChapterId}"/>
		</fo:block>
	</xsl:template>
	<xsl:template name="bookmarks">
		<fo:bookmark-tree>
			<fo:bookmark internal-destination="TOC">
				<fo:bookmark-title>
					<xsl:value-of select="$toc.var"/>
				</fo:bookmark-title>
			</fo:bookmark>
			<xsl:for-each select="//section">
				<xsl:choose>
					<xsl:when test="@sectionName='Libretto di uso e manutenzione'"/>
					<xsl:otherwise>
						<fo:bookmark internal-destination="{@id}">
							<fo:bookmark-title>
								<xsl:call-template name="SectionName"/>
							</fo:bookmark-title>
							<xsl:for-each select="current()/chapter">
							<xsl:variable name="id" select="concat('dms', @vf:dmsid)"/>
								<fo:bookmark internal-destination="{$id}">
									<fo:bookmark-title>
										<xsl:call-template name="chapterName"/>
									</fo:bookmark-title>
								</fo:bookmark>
							</xsl:for-each>
						</fo:bookmark>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</fo:bookmark-tree>
	</xsl:template>
</xsl:stylesheet>
