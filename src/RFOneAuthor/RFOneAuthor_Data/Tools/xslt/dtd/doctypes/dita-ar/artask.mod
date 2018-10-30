<!-- ============================================================= -->
<!--                    HEADER                                     -->
<!-- ============================================================= -->
<!--  MODULE:    CAP Augmented Reality Topic                       -->
<!--  VERSION:   1.0.0                                             -->
<!--  DATE:      November 2014                                     -->
<!--                                                               -->
<!-- ============================================================= -->

<!-- ============================================================= -->
<!--                    PUBLIC DOCUMENT TYPE DEFINITION            -->
<!--                    TYPICAL INVOCATION                         -->
<!--                                                               -->
<!--  Refer to this file by the following public identifier or an 
      appropriate system identifier 
PUBLIC "-//Bosch//ELEMENTS DITA AR Topic//EN"
      Delivered as file "artopic.mod"                              -->

<!-- ============================================================= -->
<!-- SYSTEM:     Darwin Information Typing Architecture (DITA)     -->
<!--                                                               -->
<!-- PURPOSE:    Declaring the elements and specialization         -->
<!--             attributes for Augmened Reality content           -->
<!--                                                               -->
<!-- ORIGINAL CREATION DATE:                                       -->
<!--             November 2014                                     -->
<!--                                                               -->
<!--  UPDATES:                                                     -->
<!-- ============================================================= -->


<!-- ============================================================= -->
<!--                   ARCHITECTURE ENTITIES                       -->
<!-- ============================================================= -->

<!-- default namespace prefix for DITAArchVersion attribute can be
     overridden through predefinition in the document type shell   -->
<!ENTITY % DITAArchNSPrefix
                       "ditaarch"                                    >

<!-- must be instanced on each topic type                          -->
<!ENTITY % arch-atts "
             xmlns:%DITAArchNSPrefix; 
                        CDATA                              #FIXED
                       'http://dita.oasis-open.org/architecture/2005/'
             %DITAArchNSPrefix;:DITAArchVersion
                        CDATA                              #FIXED
                       '1.0'"                                        >


<!-- ============================================================= -->
<!--                   SPECIALIZATION OF DECLARED ELEMENTS         -->
<!-- ============================================================= -->


<!-- ============================================================= -->
<!--                   ELEMENT NAME ENTITIES                       -->
<!-- ============================================================= -->


<!ENTITY % animation-links  "animation-links"                                   >
<!ENTITY % poi-positions  "poi-positions"                                   >
<!ENTITY % media-3d  "media-3d"                                   >
<!ENTITY % tracking-config  "tracking-config"                                   >

<!-- ============================================================= -->
<!--                    SHARED ATTRIBUTE LISTS                     -->
<!-- ============================================================= -->


<!-- Don't nest anything inside this kind of topic -->
<!ENTITY % animated-procedure-info-types "no-topic-nesting"   >


<!-- ============================================================= -->
<!--                    DOMAINS ATTRIBUTE OVERRIDE                 -->
<!-- ============================================================= -->


<!ENTITY included-domains ""                                         >


<!-- ============================================================= -->
<!--                    ELEMENT DECLARATIONS                       -->
<!-- ============================================================= -->


<!--                    LONG NAME: Animated Procedure                            -->
<!ELEMENT animated-procedure          (%title;, (%titlealts;)?, (%shortdesc;)?, 
                         (%prolog;)?, (%taskbody;), (%animation-links;), (%animated-procedure-info-types;)*) >
						 

<!--                    LONG NAME: Animation Links                      -->
<!ELEMENT animation-links      ((%media-3d;)?,(%tracking-config;)*)   >


			
<!ELEMENT media-3d EMPTY>
<!ATTLIST media-3d  
	%univ-atts; 
	outputclass CDATA  #IMPLIED    
	href       CDATA  #REQUIRED >
   

<!-- Reuse for TrackingConfiguration doctype-->
<!-- Will have vf:dmsidref attribute -->
<!ELEMENT tracking-config EMPTY >
<!ATTLIST tracking-config  
	%univ-atts; 
	outputclass CDATA  #IMPLIED    
	href       CDATA  #IMPLIED >			     
		
<!-- ============================================================= -->
<!--                    SPECIALIZATION ATTRIBUTE DECLARATIONS      -->
<!-- ============================================================= -->


<!ATTLIST animated-procedure   %global-atts;  class  CDATA "- topic/topic task/task animated-procedure/animated-procedure "        >
<!ATTLIST animation-links   %global-atts;  class  CDATA "- topic/related-links task/related-links animated-procedure/animation-links "        >
<!ATTLIST media-3d   %global-atts;  class  CDATA "- topic/link task/link animated-procedure/media-3d "  >
<!ATTLIST tracking-config   %global-atts;  class  CDATA "- topic/link task/link animated-procedure/tracking-config "  >



 
<!-- ================== End DITA ARTopic  =========================== -->
