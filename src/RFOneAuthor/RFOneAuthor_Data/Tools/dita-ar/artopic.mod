<!-- ============================================================= -->
<!--                    HEADER                                     -->
<!-- ============================================================= -->
<!--  MODULE:    CAP Augmented Reality Topic                       -->
<!--  VERSION:   1.0.1                                             -->
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
<!--    * January 2015: adding poi-3d-link element (C3D)           -->
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

<!ENTITY % poitopic   "poitopic"                                        >
<!ENTITY % poibody    "poibody"                                    	>
<!ENTITY % poi        "poi"                                       	>
<!ENTITY % poidesc    "poidesc"                                    	>
<!ENTITY % poimenu    "poimenu"                             		>
<!ENTITY % menuxref    "menuxref"                             		>
<!ENTITY % poimenuitem  "poimenuitem"                                   >
<!ENTITY % item-name  "item-name"                                   >
<!ENTITY % item-content  "item-content"                                   >
<!ENTITY % poi-links  "poi-links"                                   >
<!ENTITY % poi-positions  "poi-positions"                                   >
<!ENTITY % media-3d-poi  "media-3d-poi"                                   >
<!ENTITY % tracking-config-poi  "tracking-config-poi"                                   >
<!ENTITY % poi-3d-link  "poi-3d-link"                                   >

<!-- ============================================================= -->
<!--                    SHARED ATTRIBUTE LISTS                     -->
<!-- ============================================================= -->


<!-- Don't nest anything inside this kind of topic -->
<!ENTITY % poitopic-info-types "no-topic-nesting"   >


<!-- ============================================================= -->
<!--                    DOMAINS ATTRIBUTE OVERRIDE                 -->
<!-- ============================================================= -->


<!ENTITY included-domains ""                                         >


<!-- ============================================================= -->
<!--                    ELEMENT DECLARATIONS                       -->
<!-- ============================================================= -->


<!--                    LONG NAME: POI Topic                            -->
<!ELEMENT poitopic          (%title;, (%titlealts;)?, (%shortdesc;)?, 
                         (%prolog;)?, (%poibody;), (%poi-links;), (%poitopic-info-types;)*)  >
						 
<!ATTLIST poitopic            
             id         ID                               #IMPLIED
             conref     CDATA                            #IMPLIED
             %select-atts;
             xml:lang   NMTOKEN                          #IMPLIED
             %arch-atts;
             outputclass 
                        CDATA                            #IMPLIED
             domains    CDATA                "&included-domains;"    >


<!--                    LONG NAME: POI Body                       -->
<!ELEMENT poibody      ((%poi;)+)    >
<!ATTLIST poibody        
             %id-atts;
             translate  (yes|no)                         #IMPLIED
             xml:lang   NMTOKEN                          #IMPLIED
             outputclass 
                        CDATA                            #IMPLIED    >


<!ELEMENT poi-links      ((%media-3d-poi;)?, (%poi-positions;)?,(%tracking-config-poi;)*)    >

<!--                    LONG NAME: Poi                           -->
<!ELEMENT poi           ((%title;), (%poidesc;)?, (%poi-3d-link;)*, (%poimenu; | %menuxref;)?)      >
<!ATTLIST poi         
             %univ-atts;                                  
             outputclass 
                        CDATA                            #IMPLIED    >

<!--                    LONG NAME: Poi Description                     -->
<!ELEMENT poidesc         (%para.cnt;)*                               >
<!ATTLIST poidesc         
             %univ-atts;                                  
             outputclass 
                        CDATA                            #IMPLIED    >
			
<!--                    LONG NAME: Poi Menu Xref                     -->
<!ELEMENT menuxref         (%extxref;)*                               >
<!ATTLIST menuxref         
             %univ-atts;                                  
             outputclass 
                        CDATA                            #IMPLIED    >
						
<!--                    LONG NAME: Poi Menu                         -->
<!ELEMENT poimenu         ((%poimenuitem;)+)                                  >
<!ATTLIST poimenu         
             %univ-atts;                                  
             outputclass 
                        CDATA                            #IMPLIED    >
			

<!--                    LONG NAME: Poi Menu Item                      -->
<!ELEMENT poimenuitem         ((%item-name;), (%extxref; | %item-content;)?)      >
<!ATTLIST poimenuitem         
             %univ-atts;                                  
             outputclass 
                        CDATA                            #IMPLIED    >

<!ELEMENT item-name         (%ph.cnt;)*                                >
<!ATTLIST item-name         
             %univ-atts;                                  
             outputclass 
                        CDATA                            #IMPLIED    >

<!ELEMENT item-content         (%basic.block;)*                                >
<!ATTLIST item-content         
             %univ-atts;                                  
             outputclass 
                        CDATA                            #IMPLIED    >			
<!ELEMENT media-3d-poi EMPTY>
<!ATTLIST media-3d-poi  
	%univ-atts; 
	outputclass CDATA  #IMPLIED    
	href       CDATA  #REQUIRED >
		     
<!-- Reuse for POIPositions doctype-->	
<!-- Actual content should be X3D -->
<!-- Will have vf:dmsidref attribute -->
<!ELEMENT poi-positions EMPTY >	 
<!ATTLIST poi-positions  
	%univ-atts; 
	outputclass CDATA  #IMPLIED    
	href       CDATA  #IMPLIED >	    

<!-- Reuse for TrackingConfiguration doctype-->
<!-- Will have vf:dmsidref attribute -->
<!ELEMENT tracking-config-poi EMPTY >		
<!ATTLIST tracking-config-poi  
	%univ-atts; 
	outputclass CDATA  #IMPLIED    
	href       CDATA  #IMPLIED >		
	
<!-- Link to the object inside 3D -->
<!ELEMENT poi-3d-link (%param;)*>
<!ATTLIST poi-3d-link  
	%univ-atts; 
	outputclass CDATA  #IMPLIED>
	
<!-- ============================================================= -->
<!--                    SPECIALIZATION ATTRIBUTE DECLARATIONS      -->
<!-- ============================================================= -->


<!ATTLIST poitopic   %global-atts;  class  CDATA "- topic/topic poitopic/poitopic "        >
<!ATTLIST poibody    %global-atts;  class  CDATA "- topic/body poitopic/poibody "     >
<!ATTLIST poi       %global-atts;  class  CDATA "- topic/section poitopic/poi "          >
<!ATTLIST poimenu   %global-atts;  class  CDATA "- topic/ul poitopic/poimenu "          >
<!ATTLIST poidesc   %global-atts;  class  CDATA "- topic/p poitopic/poidesc "          >
<!ATTLIST menuxref   %global-atts;  class  CDATA "- topic/p poitopic/menuxref "          >
<!ATTLIST poimenuitem   %global-atts;  class  CDATA "- topic/li poitopic/poimenuitem "        >
<!ATTLIST item-name   %global-atts;  class  CDATA "- topic/ph poitopic/item-name "        >
<!ATTLIST item-content   %global-atts;  class  CDATA "- topic/itemgroup poitopic/item-content "        >
<!ATTLIST poi-links   %global-atts;  class  CDATA "- topic/related-links poitopic/poi-links"        >
<!ATTLIST tracking-config-poi   %global-atts;  class  CDATA "- topic/link poitopic/tracking-config-poi "        >
<!ATTLIST media-3d-poi   %global-atts;  class  CDATA "- topic/link poitopic/media-3d-poi "        >
<!ATTLIST poi-positions   %global-atts;  class  CDATA "- topic/link poitopic/poi-positions "        >
<!ATTLIST poi-3d-link   %global-atts;  class  CDATA "- topic/object poitopic/poi-3d-link "        >
 
<!-- ================== End DITA ARTopic  =========================== -->
