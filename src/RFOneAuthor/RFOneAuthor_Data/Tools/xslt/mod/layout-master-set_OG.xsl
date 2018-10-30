<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:axf="http://www.antennahouse.com/names/XSL/Extensions" xmlns:vf="http://xmlcompass.vftis.com/namespace/vfreuse" exclude-result-prefixes="xlink xs fn xdt" extension-element-prefixes="axf">
	<xsl:template name="layout-master.psu">
		<fo:layout-master-set>
			<fo:simple-page-master master-name="COVERpage" page-height="112.2mm" page-width="155.6mm" margin-top="14.2mm" margin-left="14.6mm" margin-bottom="5mm">
				<fo:region-body margin-bottom="7.6mm" region-name="xsl-region-body"/>
				<fo:region-before region-name="cover-region-before" extent="0mm"/>
				<fo:region-after extent="6mm" region-name="cover-region-footer"/>
			</fo:simple-page-master>
			<!-- left page-->
			<fo:simple-page-master master-name="body-recto" margin-left="10.8mm" margin-right="10.8mm" margin-top="10.6mm" page-height="112.2mm" page-width="155.6mm" margin-bottom="5mm">
				<fo:region-body margin-bottom="7.6mm" region-name="xsl-region-body" column-count="2" column-gap="3mm" display-align="auto"/>
				<fo:region-after extent="6mm" region-name="even-footer"/>
			</fo:simple-page-master>
			<!-- right page-->
			<fo:simple-page-master master-name="body-verso" margin-left="10.8mm" margin-right="10.8mm" margin-top="10.6mm" page-height="112.2mm" page-width="155.6mm" margin-bottom="5mm">
				<fo:region-body margin-bottom="7.6mm" region-name="xsl-region-body" column-count="2" column-gap="3mm"/>
				<fo:region-after extent="6mm" region-name="odd-footer"/>
			</fo:simple-page-master>
			<fo:flow-map flow-map-name="extra-flow">
				<fo:flow-assignment>
					<fo:flow-source-list>
						<fo:flow-name-specifier flow-name-reference="body-wide"/>
					</fo:flow-source-list>
					<fo:flow-target-list>
						<fo:region-name-specifier region-name-reference="body-wide"/>
					</fo:flow-target-list>
				</fo:flow-assignment>
			</fo:flow-map>
			<fo:page-sequence-master master-name="pageSequence">
				<fo:repeatable-page-master-alternatives>
					<fo:conditional-page-master-reference odd-or-even="even" master-reference="body-recto" page-position="rest"/>
					<fo:conditional-page-master-reference odd-or-even="odd" master-reference="body-verso" page-position="rest"/>
				</fo:repeatable-page-master-alternatives>
			</fo:page-sequence-master>
		</fo:layout-master-set>
	</xsl:template>
	<!-- cover page settings-->
	<xsl:template name="CoverImage">
		<fo:block-container position="absolute" height="9.50mm" width="80mm">
			<fo:block font-family="{$fontGeneral}" font-size="10pt">
				<xsl:value-of select="$doctype.var"/>
			</fo:block>
		</fo:block-container>
		<fo:block-container position="absolute" background-color="black" height="9.50mm" width="32mm" left="96mm">
			<fo:block-container position="absolute" top="2mm" height="7mm" width="28mm" left="1.3mm" font-family="{$fontGeneral}" font-size="12pt" text-align="center">
				<fo:block color="white">
					<xsl:value-of select="$lang.var"/>
				</fo:block>
			</fo:block-container>
		</fo:block-container>
		<fo:block margin-top="31mm">
			<xsl:variable name="graphic">
				<xsl:choose>
					<xsl:when test="$family='M'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='M.000']//figure[1]/graphic/@name"/>
					</xsl:when>
					<xsl:when test="$family='SBK'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='SBK.000']//figure[1]/graphic/@name"/>
					</xsl:when>
					<xsl:when test="$family='F'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='F.000']//figure[1]/graphic/@name"/>
					</xsl:when>
					<xsl:when test="$family='HYM'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='HYM.000']//figure[1]/graphic/@name"/>
					</xsl:when>
					<xsl:when test="$family='D'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='D.000']//figure[1]/graphic/@name"/>
					</xsl:when>
					<xsl:when test="$family='MTS'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='MTS.000']//figure[1]/graphic/@name"/>
					</xsl:when>
				</xsl:choose>
			</xsl:variable>
			<xsl:variable name="width">
				<xsl:choose>
					<xsl:when test="$family='M'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='M.000']//figure[1]/graphic/@width"/>
					</xsl:when>
					<xsl:when test="$family='SBK'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='SBK.000']//figure[1]/graphic/@width"/>
					</xsl:when>
					<xsl:when test="$family='F'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='F.000']//figure[1]/graphic/@width"/>
					</xsl:when>
					<xsl:when test="$family='HYM'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='HYM.000']//figure[1]/graphic/@width"/>
					</xsl:when>
					<xsl:when test="$family='D'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='D.000']//figure[1]/graphic/@width"/>
					</xsl:when>
					<xsl:when test="$family='MTS'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='MTS.000']//figure[1]/graphic/@width"/>
					</xsl:when>
				</xsl:choose>
			</xsl:variable>
			<xsl:variable name="height">
				<xsl:choose>
					<xsl:when test="$family='M'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='M.000']//figure[1]/graphic/@height"/>
					</xsl:when>
					<xsl:when test="$family='SBK'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='SBK.000']//figure[1]/graphic/@height"/>
					</xsl:when>
					<xsl:when test="$family='F'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='F.000']//figure[1]/graphic/@height"/>
					</xsl:when>
					<xsl:when test="$family='HYM'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='HYM.000']//figure[1]/graphic/@height"/>
					</xsl:when>
					<xsl:when test="$family='D'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='D.000']//figure[1]/graphic/@height"/>
					</xsl:when>
					<xsl:when test="$family='MTS'">
						<xsl:value-of select="owners-guide/section[@sectionNumber='MTS.000']//figure[1]/graphic/@height"/>
					</xsl:when>
				</xsl:choose>
			</xsl:variable>
			<fo:external-graphic src="{$img_path}{$graphic}" content-width="{$width}" content-height="{$height}" scaling="uniform"/>
		</fo:block>
	</xsl:template>
</xsl:stylesheet>
