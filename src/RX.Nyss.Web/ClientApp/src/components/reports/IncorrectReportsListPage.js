import styles from "./ReportsListPage.module.scss";

import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as reportsActions from './logic/reportsActions';
import TableActions from '../common/tableActions/TableActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import IncorrectReportsTable from './IncorrectReportsTable';
import { useMount } from '../../utils/lifecycle';
import { ReportFilters } from '../common/filters/ReportFilters';
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/tableActions/TableActionsButton";
import { Hidden, Icon } from "@material-ui/core";
import * as roles from '../../authentication/roles';
import { SendReportDialog } from "./SendReportDialog";
import { useState } from "react";

const Page = "incorrect";

const IncorrectReportsListPageComponent = (props) => {
  const [open, setOpen] = useState(false);

  const canSendReport = props.user && [roles.Administrator, roles.Manager, roles.TechnicalAdvisor, roles.Supervisor, roles.HeadSupervisor]
    .some(neededRole => props.user.roles.some(userRole => userRole === neededRole));

  useMount(() => {
    props.openReportsList(props.projectId);
  });

  if (!props.data || !props.filters || !props.sorting) {
    return null;
  }

  const handleRefresh = () =>
    props.getList(props.projectId, 1);

  const handleFiltersChange = (filters) =>
    props.getList(props.projectId, 1, filters, props.sorting);

  const handlePageChange = (page) =>
    props.getList(props.projectId, page, props.filters, props.sorting);

  const handleSortChange = (sorting) =>
    props.getList(props.projectId, props.page, props.filters, sorting);

  const handleSendReport = (e) => {
    e.stopPropagation();
    props.openSendReport(props.projectId);
    setOpen(true);
  }

  function exportToCsv() {
    props.trackReportExport(Page, "Csv", props.projectId);
    props.exportToCsv(props.projectId, props.filters, props.sorting);
  }

  function exportToExcel() {
    props.trackReportExport(Page, "Excel", props.projectId);
    props.exportToExcel(props.projectId, props.filters, props.sorting);
  } 

  return (
    <Fragment>
      <TableActions>
        <Hidden xsDown>
          <TableActionsButton onClick={handleRefresh} isFetching={props.isListFetching}>
            <Icon>refresh</Icon>
          </TableActionsButton>
        </Hidden>

        <TableActionsButton onClick={exportToCsv}>
          {strings(stringKeys.reports.list.exportToCsv)}
        </TableActionsButton>

        <TableActionsButton onClick={exportToExcel}>
          {strings(stringKeys.reports.list.exportToExcel)}
        </TableActionsButton>

        {canSendReport &&
          <TableActionsButton onClick={handleSendReport}>
            {strings(stringKeys.reports.list.sendReport)}
          </TableActionsButton>
        }
      </TableActions>

      {open && (
        <SendReportDialog close={() => setOpen(false)}
          projectId={props.projectId}
          openSendReport={props.openSendReport}
          sendReport={props.sendReport} />
      )}

      <div className={styles.filtersGrid}>
        <ReportFilters
          nationalSocietyId={props.nationalSocietyId}
          onChange={handleFiltersChange}
          filters={props.filters}
          showCorrectReportFilters={false}
        />
      </div>

      <IncorrectReportsTable
        list={props.data.data}
        isListFetching={props.isListFetching}
        page={props.data.page}
        onChangePage={handlePageChange}
        totalRows={props.data.totalRows}
        rowsPerPage={props.data.rowsPerPage}
        reportsType={props.filters.reportsType}
        filters={props.filters}
        sorting={props.sorting}
        onSort={handleSortChange}
        user={props.user}
      />
    </Fragment>
  );
}

IncorrectReportsListPageComponent.propTypes = {
  getReports: PropTypes.func,
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: state.appData.siteMap.parameters.nationalSocietyId,
  data: state.reports.incorrectReportsPaginatedListData,
  isListFetching: state.reports.listFetching,
  isRemoving: state.reports.listRemoving,
  filters: state.reports.incorrectReportsFilters,
  sorting: state.reports.incorrectReportsSorting,
  user: state.appData.user
});

const mapDispatchToProps = {
  openReportsList: reportsActions.openIncorrectReportsList.invoke,
  getList: reportsActions.getIncorrectList.invoke,
  exportToExcel: reportsActions.exportToExcel.invoke,
  exportToCsv: reportsActions.exportToCsv.invoke,
  openSendReport: reportsActions.openSendReport.invoke,
  sendReport: reportsActions.sendReport.invoke,
  trackReportExport: reportsActions.trackReportExport,
};

export const IncorrectReportsListPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(IncorrectReportsListPageComponent)
);
