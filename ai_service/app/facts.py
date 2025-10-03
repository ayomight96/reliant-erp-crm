from __future__ import annotations
from typing import Optional

PRODUCT_FACTS = {
    ("window", "uPVC", "double"): [
        "A+ rated double glazing as standard on uPVC windows and doors",
        "FENSA-compliant installation by accredited fitters",
        "Trickle vents and egress options can be specified",
    ],
    ("window", "uPVC", "triple"): [
        "A+ energy performance with triple glazing available",
        "FENSA-compliant installation by accredited fitters",
    ],
    ("window", "Aluminium", "double"): [
        "Slim sightlines with thermally broken frames for energy efficiency",
        "High durability, low maintenance; resists warping and corrosion",
        "Multi-point locking for enhanced security",
    ],
    ("window", "Aluminium", "triple"): [
        "Thermal break technology plus triple glazing for superior insulation",
        "Slim profiles maximise glass area and natural light",
    ],
    ("window", "Aluminium", None): [
        "Customisable styles, colours and finishes; made to measure",
        "Engineered for long service life (often 30+ years with simple care)",
    ],
    ("door", "Composite", "double"): [
        "GRP skins with timber-look finish and low maintenance",
        "Multi-point locking; PAS 24 & Secured by Design options",
    ],
    ("door", "Aluminium", None): [
        "Slim frames allow larger glass panels and brighter interiors",
        "Advanced multi-point locking and robust construction",
        "Thermal break + high-performance glazing improve efficiency",
    ],
    ("door", None, None): [
        "Energy-efficient designs with modern weather seals to cut draughts",
        "Custom colours, hardware and glazing options to match the property",
    ],
    ("conservatory", None, None): [
        "Year-round comfort with energy-efficient glazing options",
        "Team Guardian Warm Roof systems available for superior insulation",
        "On-site showroom village in Studley to view builds in person",
    ],
    ("window", None, None): [
        "A+ rated double or triple glazing options to reduce heat loss",
        "Made-to-measure, with styles and hardware tailored to your home",
    ],
}

ACCREDITATIONS = [
    "FENSA Certified company with MTC-qualified installers and surveyors",
    "Consumer Protection Association member with 95–100% survey scores over 15+ years",
    "CHAS and Constructionline accredited for health & safety and quality",
    "FCA-regulated credit broker offering flexible finance via a panel of lenders",
]

SECURITY_TECH = {
    "Kubu": [
        "Kubu Smart Security built-in as standard on new doors/windows",
        "Proactive alerts before intrusion; geofence ‘SureSecure’ reminders",
        "Data security: Secured by Design Secure Connected Device, PSTI compliant, IASME Level 2, ETSI EN 303-645",
    ],
    "Avocet ATK": [
        "Sold Secure Diamond, 3-Star Kitemmarked anti-snap cylinder",
        "Patented ‘Click Secure’ cam; anti-drill/pick/bump",
    ],
    "Avantis AutoFire": [
        "Kubu-ready AutoLock with sensor, PAS24:2022 load-tested",
        "Secured by Design accredited; 10-year mechanical guarantee",
    ],
}


def gather_facts(
    product_type: Optional[str], material: Optional[str], glazing: Optional[str]
) -> list[str]:
    keys = [
        (product_type, material, glazing),
        (product_type, material, None),
        (product_type, None, glazing),
        (product_type, None, None),
    ]
    seen, out = set(), []
    for k in keys:
        for s in PRODUCT_FACTS.get(k, []):
            if s not in seen:
                seen.add(s)
                out.append(s)
    return out
