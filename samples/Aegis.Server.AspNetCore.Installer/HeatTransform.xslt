<?xml version="1.0" encoding="utf-8"?>
<!--
  Injects ServiceInstall / ServiceControl into the harvested component that owns Aegis.Server.AspNetCore.exe.
  Service name must match WindowsServiceInfo.ServiceName (AegisLicensingServer).
-->
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
    xmlns="http://schemas.microsoft.com/wix/2006/wi"
    exclude-result-prefixes="wix">

  <xsl:output method="xml" indent="yes" encoding="utf-8" />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:Component[wix:File[contains(@Source, 'Aegis.Server.AspNetCore.exe')]]">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
      <ServiceInstall
          Id="AegisLicensingServiceInstall"
          Name="$(var.WindowsServiceName)"
          DisplayName="$(var.ServiceDisplayName)"
          Description="FTT Aegis licencing admin UI and API"
          Type="ownProcess"
          Start="auto"
          Account="LocalSystem"
          ErrorControl="normal"
          Vital="yes"
          Interactive="no" />
      <ServiceControl
          Id="AegisLicensingServiceControl"
          Name="$(var.WindowsServiceName)"
          Start="install"
          Stop="both"
          Remove="uninstall"
          Wait="yes" />
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>
