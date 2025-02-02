import React from 'react';
import { strings, stringKeys } from "../../../strings";
import SubmitButton from "../../forms/submitButton/SubmitButton";
import {
  useTheme,
  DialogTitle,
  Dialog,
  Button,
  DialogContent,
  useMediaQuery,
  Typography,
} from "@material-ui/core";
import FormActions from "../../forms/formActions/FormActions";

export const AlertsCloseDialog = ({ isOpened, close, alertId, isClosing, closeAlert }) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('xs'));

  const handleClose = (event) => {
    event.preventDefault();
    closeAlert(alertId);
  }

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogTitle>{strings(stringKeys.alerts.assess.alert.closeConfirmation)}</DialogTitle>
      <DialogContent>
        <Typography variant="body1">{strings(stringKeys.alerts.assess.alert.closeDescription)}</Typography>
        <FormActions>
          <Button onClick={close}>
            {strings(stringKeys.form.cancel)}
          </Button>
          <SubmitButton isFetching={isClosing} onClick={handleClose}>
            {strings(stringKeys.alerts.assess.alert.close)}
          </SubmitButton>
        </FormActions>
      </DialogContent>
    </Dialog>
  );
}
