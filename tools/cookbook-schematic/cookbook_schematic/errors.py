class SchematicError(Exception):
    """Base class for actionable schematic input errors."""


class NetlistError(SchematicError):
    """Raised when a SPICE netlist cannot be summarized safely."""


class LayoutError(SchematicError):
    """Raised when a schematic layout is invalid."""
