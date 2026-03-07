# .APPENDMODEL Statement

The `.APPENDMODEL` statement appends additional parameters to an existing model definition, typically used to add tolerance or distribution information for Monte Carlo analysis.

## Syntax

```
.APPENDMODEL <model_name> <model_type> (<param1>=<value1> [<param2>=<value2> ...])
```

## Example

```spice
* Define base model
.model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752)

* Append tolerance info
.APPENDMODEL 1N914 D (Is=LOT=10% Rs=LOT=5%)
```

## Notes

- The model must already be defined before `.APPENDMODEL` is used.
- This statement is primarily used with Monte Carlo (`.MC`) to add lot-to-lot and device-to-device variation parameters.
