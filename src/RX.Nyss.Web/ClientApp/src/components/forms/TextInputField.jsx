import React from "react";
import PropTypes from "prop-types";
import { createFieldComponent } from "./FieldBase";
import { TextField } from '@material-ui/core';

const TextInput = ({ error, name, label, value, controlProps, multiline, rows, autoWidth, autoFocus, disabled, type, inputMode, endAdornment }) => {
  return (
    <TextField
      name={name}
      error={!!error}
      helperText={error}
      label={label}
      value={value}
      multiline={multiline}
      rows={rows}
      disabled={disabled}
      fullWidth={autoWidth ? false : true}
      InputLabelProps={{ shrink: true }}
      InputProps={{ 
        ...controlProps,
        endAdornment: !!endAdornment && endAdornment
      }}
      inputProps={{ autoFocus: autoFocus, inputMode: inputMode, step: type === "number" ? "any" : null }}
      type={type}
    />
  );
};

TextInput.propTypes = {
  controlProps: PropTypes.object,
  name: PropTypes.string
};

export const TextInputField = createFieldComponent(TextInput);
export default TextInputField;
