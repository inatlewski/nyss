import { useState, Fragment, useMemo, useCallback, useEffect } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import { MenuItem, Button, Grid } from "@material-ui/core";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { userRoles, globalCoordinatorUserRoles, coordinatorUserRoles, headManagerRoles, sexValues } from './logic/nationalSocietyUsersConstants';
import * as roles from '../../authentication/roles';
import SelectField from '../forms/SelectField';
import { ValidationMessage } from '../forms/ValidationMessage';
import { ConfirmationDialog } from '../common/confirmationDialog/ConfirmationDialog';
import { getBirthDecades, parseBirthDecade } from '../../utils/birthYear';

const NationalSocietyUsersCreatePageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [selectedRole, setRole] = useState(null);
  const [confirmCoordinatorDialog, setConfirmCoordinatorDialog] = useState({
    isOpened: false,
    isConfirmed: false
  });

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  });

  const hasAnyRole = useCallback((...roles) =>
    props.callingUserRoles.some(userRole => roles.some(role => role === userRole)),
    [props.callingUserRoles]
  );

  const canChangeOrganization = useMemo(() =>
    (hasAnyRole(roles.Administrator, roles.Coordinator) && selectedRole !== roles.DataConsumer)
    || (hasAnyRole(roles.GlobalCoordinator) && selectedRole === roles.Coordinator)
    || (props.data && props.data.isHeadManager && !props.data.hasCoordinator && selectedRole === roles.Coordinator),
    [hasAnyRole, selectedRole, props.data]);

  const canSelectModem = useMemo(() =>
    (selectedRole === roles.Manager
      || selectedRole === roles.TechnicalAdvisor
      || selectedRole === roles.HeadSupervisor
      || selectedRole === roles.Supervisor)
    && props.data && props.data.modems.length > 0,
    [props.data, selectedRole]);

  const availableUserRoles = useMemo(() => {
    if (!props.data) {
      return [];
    }

    if (props.callingUserRoles.some(r => r === roles.Administrator)) {
      return headManagerRoles;
    }

    if (hasAnyRole(roles.GlobalCoordinator)) {
      return globalCoordinatorUserRoles.filter(r => !props.data.hasCoordinator || r !== roles.Coordinator);
    }

    if (hasAnyRole(roles.Coordinator)) {
      if (props.data.organizations.every((o) => o.hasHeadManager)) {
        return [roles.Coordinator];
      }

      return coordinatorUserRoles;
    }

    if (props.data.isHeadManager) {
      return headManagerRoles.filter(r => !props.data.hasCoordinator || r !== roles.Coordinator);
    }

    return userRoles;
  }, [hasAnyRole, props.callingUserRoles, props.data]);

  const availableOrganizations = useMemo(() => {
    if (!props.data) {
      return [];
    }

    if (hasAnyRole(roles.Coordinator) && selectedRole === roles.Manager) {
      return props.data.organizations.filter((o) => !o.hasHeadManager);
    }

    return props.data.organizations;
  }, [hasAnyRole, props.data, selectedRole]);

  const form = useMemo(() => {
    const fields = {
      nationalSocietyId: parseInt(props.nationalSocietyId),
      role: '',
      name: '',
      email: '',
      phoneNumber: '',
      additionalPhoneNumber: '',
      organization: '',
      decadeOfBirth: '',
      projectId: '',
      sex: '',
      organizationId: '',
      headSupervisorId: '',
      modemId: ''
    };

    const validation = {
      role: [validators.required],
      name: [validators.required, validators.maxLength(100)],
      email: [validators.required, validators.email, validators.maxLength(100)],
      phoneNumber: [validators.required, validators.maxLength(20), validators.phoneNumber],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      organization: [validators.requiredWhen(f => f.role === roles.DataConsumer), validators.maxLength(100)],
      decadeOfBirth: [validators.requiredWhen(f => f.role === roles.Supervisor || f.role === roles.HeadSupervisor)],
      sex: [validators.requiredWhen(f => f.role === roles.Supervisor || f.role === roles.HeadSupervisor)],
      projectId: [validators.requiredWhen(f => f.role === roles.Supervisor || f.role === roles.HeadSupervisor)]
    };

    const newForm = createForm(fields, validation);
    newForm.fields.role.subscribe(({ newValue }) => setRole(newValue));

    return newForm;
  }, [props.nationalSocietyId]);

  useEffect(() => {
    if (!form || selectedRole) {
      return;
    }

    const newRole = availableUserRoles.some(r => r === roles.Manager) ? roles.Manager : availableUserRoles[0];
    setRole(newRole);
    form.fields.role.update(newRole);
  }, [availableUserRoles, selectedRole, form]);

  useEffect(() => {
    form && form.fields.organizationId.setValidators([validators.requiredWhen(_ => canChangeOrganization)]);
  }, [form, canChangeOrganization]);

  useEffect(() => {
    form && form.fields.modemId.setValidators([validators.requiredWhen(_ => canSelectModem)]);
  }, [form, canSelectModem]);

  useEffect(() => {
    if (!form) {
      return;
    }

    const organizationId = availableOrganizations.some(o => o.isDefaultOrganization) ?
      availableOrganizations.filter(o => o.isDefaultOrganization)[0].id.toString()
      : (availableOrganizations.length > 0 && availableOrganizations[0].id.toString()) || '';

    form.fields.organizationId.update(organizationId);
  }, [availableOrganizations, availableUserRoles, form]);


  useCustomErrors(form, props.error);

  const { create } = props;

  const createUser = useCallback(() => {
    const values = form.getValues();
    create(props.nationalSocietyId, {
      ...values,
      organizationId: (canChangeOrganization && values.organizationId) ? parseInt(values.organizationId) : null,
      projectId: values.projectId ? parseInt(values.projectId) : null,
      decadeOfBirth: values.decadeOfBirth ? parseInt(values.decadeOfBirth) : null,
      setAsHeadManager: hasAnyRole(roles.Coordinator, roles.GlobalCoordinator) ? true : null,
      headSupervisorId: values.headSupervisorId ? parseInt(values.headSupervisorId) : null,
      modemId: !!values.modemId ? parseInt(values.modemId) : null
    });
  }, [hasAnyRole, canChangeOrganization, form, create, props.nationalSocietyId]);

  const handleSubmit = useCallback(e => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    if (selectedRole === roles.Coordinator && !props.data.hasCoordinator && confirmCoordinatorDialog.isConfirmed === false) {
      setConfirmCoordinatorDialog({ ...confirmCoordinatorDialog, isOpened: true });
      return;
    }

    createUser();
  }, [createUser, form, selectedRole, props.data, confirmCoordinatorDialog]);

  if (!props.data) {
    return null;
  }

  const confirmCoordinatorCreation = () => {
    setConfirmCoordinatorDialog({ ...confirmCoordinatorDialog, isConfirmed: true, isOpened: false });
    createUser();
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.email)}
              name="email"
              field={form.fields.email}
              autoFocus
              inputMode={"email"}
            />
          </Grid>

          <Grid item xs={12}>
            <SelectInput
              label={strings(stringKeys.nationalSocietyUser.form.role)}
              name="role"
              field={form.fields.role}
            >
              {availableUserRoles.map(role => (
                <MenuItem
                  key={`role${role}`}
                  value={role}>
                  {strings(`role.${((hasAnyRole(roles.Coordinator, roles.GlobalCoordinator) && role === roles.Manager) ? "headManager" : role).toLowerCase()}`)}
                </MenuItem>
              ))}
            </SelectInput>
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.phoneNumber)}
              name="phoneNumber"
              field={form.fields.phoneNumber}
              inputMode={"tel"}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.nationalSocietyUser.form.additionalPhoneNumber)}
              name="additionalPhoneNumber"
              field={form.fields.additionalPhoneNumber}
              inputMode={"tel"}
            />
          </Grid>
          {canChangeOrganization && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.organization)}
                field={form.fields.organizationId}
                name="organizationId"
                customProps={{
                  disabled: selectedRole === roles.Coordinator && hasAnyRole(roles.GlobalCoordinator)
                }}
              >
                {availableOrganizations.map(organization => (
                  <MenuItem key={`organization_${organization.id}`} value={organization.id.toString()}>
                    {organization.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}
          {selectedRole !== roles.Coordinator && (
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.nationalSocietyUser.form.customOrganization)}
                name="organization"
                field={form.fields.organization}
              />
            </Grid>
          )}

          {(selectedRole === roles.Supervisor || selectedRole === roles.HeadSupervisor) && (
            <Fragment>
              <Grid item xs={12}>
                <SelectField
                  label={strings(stringKeys.nationalSocietyUser.form.decadeOfBirth)}
                  field={form.fields.decadeOfBirth}
                  name="decadeOfBirth"
                >
                  {birthDecades.map(decade => (
                    <MenuItem key={`birthDecade_${decade}`} value={decade}>
                      {parseBirthDecade(decade)}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
              <Grid item xs={12}>
                <SelectField
                  label={strings(stringKeys.nationalSocietyUser.form.sex)}
                  field={form.fields.sex}
                  name="sex"
                >
                  {sexValues.map(type => (
                    <MenuItem key={`sex${type}`} value={type}>
                      {strings(stringKeys.dataCollector.constants.sex[type.toLowerCase()])}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
              <Grid item xs={12}>
                <SelectField
                  label={strings(stringKeys.nationalSocietyUser.form.project)}
                  field={form.fields.projectId}
                  name="projectId"
                >
                  {props.data.projects.map(project => (
                    <MenuItem key={`project_${project.id}`} value={project.id.toString()}>
                      {project.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
            </Fragment>
          )}

          {selectedRole === roles.Supervisor && props.data.headSupervisors.length > 0 && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.headSupervisor)}
                field={form.fields.headSupervisorId}
                name="headSupervisorId"
              >
                {props.data.headSupervisors.map(headSupervisor => (
                  <MenuItem key={`headSupervisor_${headSupervisor.id}`} value={headSupervisor.id.toString()}>
                    {headSupervisor.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {canSelectModem && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.modem)}
                field={form.fields.modemId}
                name="modemId"
              >
                {props.data.modems.map(modem => (
                  <MenuItem key={`modemId_${modem.id}`} value={modem.id.toString()}>
                    {modem.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.nationalSocietyUser.form.create)}</SubmitButton>
        </FormActions>
      </Form>

      <ConfirmationDialog
        isOpened={confirmCoordinatorDialog.isOpened}
        titleText={strings(stringKeys.nationalSocietyUser.form.confirmCoordinatorCreation)}
        submit={() => confirmCoordinatorCreation(handleSubmit)}
        close={() => setConfirmCoordinatorDialog({ ...confirmCoordinatorDialog, isOpened: false })}
        contentText={strings(stringKeys.nationalSocietyUser.form.confirmCoordinatorCreationText)}
      />
    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  data: state.nationalSocietyUsers.formAdditionalData,
  isSaving: state.nationalSocietyUsers.formSaving,
  error: state.nationalSocietyUsers.formError,
  callingUserRoles: state.appData.user.roles
});

const mapDispatchToProps = {
  openCreation: nationalSocietyUsersActions.openCreation.invoke,
  create: nationalSocietyUsersActions.create.invoke,
  goToList: nationalSocietyUsersActions.goToList
};

export const NationalSocietyUsersCreatePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersCreatePageComponent)
);
