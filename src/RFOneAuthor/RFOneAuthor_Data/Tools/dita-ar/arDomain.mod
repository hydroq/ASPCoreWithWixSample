<!-- ============================================================= -->
<!--                    HEADER                                     -->
<!-- ============================================================= -->
<!--  MODULE:    DITA Augmented Reality Domain                        -->
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
PUBLIC "-//Bosch//ELEMENTS DITA Augmented Reality Domain//EN"
      Delivered as file "arDomain.mod"                             -->

<!-- ============================================================= -->
<!-- SYSTEM:     Darwin Information Typing Architecture (DITA)     -->
<!--                                                               -->
<!-- PURPOSE:    Declaring the elements and specialization         -->
<!--             attributes for the User Interface Domain          -->
<!--                                                               -->
<!-- ORIGINAL CREATION DATE:                                       -->
<!--             November 2014                                        -->
<!--                                                               -->
<!--             (C) Copyright Bosch 2014.                    -->
<!--             All Rights Reserved.                              -->
<!--                                                               -->
<!--  UPDATES:                                                     -->
<!-- ============================================================= -->


<!-- ============================================================= -->
<!--                   ELEMENT NAME ENTITIES                       -->
<!-- ============================================================= -->

  
<!ENTITY % shop-link   "shop-link"                                   >
<!ENTITY % manual-link    "manual-link"                    >


<!-- ============================================================= -->
<!--                    UI KEYWORD TYPES ELEMENT DECLARATIONS      -->
<!-- ============================================================= -->

<!ELEMENT shop-link (%xreftext.cnt;)* >
<!ATTLIST shop-link  href       CDATA  #REQUIRED
		     scope CDATA "external"
                     id CDATA #IMPLIED >
		     
<!ELEMENT manual-link (%xreftext.cnt;)* >
<!ATTLIST manual-link  href       CDATA  #REQUIRED
		     scope  CDATA "external"
                     id CDATA #IMPLIED >

<!-- ============================================================= -->
<!--                    SPECIALIZATION ATTRIBUTE DECLARATIONS      -->
<!-- ============================================================= -->
             

<!ATTLIST shop-link %global-atts;  class CDATA "+ topic/xref ar-d/shop-link "  >
<!ATTLIST manual-link      %global-atts;  class CDATA "+ topic/xref ar-d/manual-link "      >


 
<!-- ================== End DITA Augmented Reality Domain =========== -->
