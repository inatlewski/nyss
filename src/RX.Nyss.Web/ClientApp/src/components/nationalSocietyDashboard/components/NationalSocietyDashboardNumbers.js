import styles from "./NationalSocietyDashboardNumbers.module.scss";

import React from 'react';
import Grid from '@material-ui/core/Grid';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import { Loading } from '../../common/loading/Loading';
import { stringKeys, strings } from '../../../strings';

export const NationalSocietyDashboardNumbers = ({ isFetching, summary, reportsType }) => {
  if (isFetching || !summary) {
    return <Loading />;
  }

  const renderNumber = (label, value) => (
    <Grid container spacing={2}>
      <Grid item className={styles.numberName}>{label}</Grid>
      <Grid item className={styles.numberValue}>{value}</Grid>
    </Grid>
  );

  return (
    <Grid container spacing={2}>
      <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.nationalSociety.dashboard.numbers.totalReportCountTitle)} />
          <CardContent>
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.numbers.keptReportCount), summary.keptReportCount)}
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.numbers.dismissedReportCount), summary.dismissedReportCount)}
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.numbers.totalReportCount), summary.reportCount)}
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.numbers.totalErrorReportCount), summary.errorReportCount)}
          </CardContent>
        </Card>
      </Grid>

      <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.nationalSociety.dashboard.dataCollectors)} />
          <CardContent>
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.activeDataCollectorCount), summary.activeDataCollectorCount)}
          </CardContent>
        </Card>
      </Grid>

      {reportsType === "dataCollectionPoint" && (
        <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
          <Card className={styles.card}>
            <CardHeader title={strings(stringKeys.nationalSociety.dashboard.dataCollectionPoints)} />
            <CardContent>
              {renderNumber(strings(stringKeys.nationalSociety.dashboard.referredToHospitalCount), summary.dataCollectionPointSummary.referredToHospitalCount)}
              {renderNumber(strings(stringKeys.nationalSociety.dashboard.fromOtherVillagesCount), summary.dataCollectionPointSummary.fromOtherVillagesCount)}
              {renderNumber(strings(stringKeys.nationalSociety.dashboard.deathCount), summary.dataCollectionPointSummary.deathCount)}
            </CardContent>
          </Card>
        </Grid>
      )}

      {reportsType !== "dataCollectionPoint" && (
        <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
          <Card className={styles.card}>
            <CardHeader title={strings(stringKeys.nationalSociety.dashboard.numbers.alertsSummaryTitle)} />
            <CardContent>
              {renderNumber(strings(stringKeys.project.dashboard.numbers.openAlerts), summary.alertsSummary.open)}
              {renderNumber(strings(stringKeys.project.dashboard.numbers.escalatedAlerts), summary.alertsSummary.escalated)}
              {renderNumber(strings(stringKeys.project.dashboard.numbers.closedAlerts), summary.alertsSummary.closed)}
              {renderNumber(strings(stringKeys.project.dashboard.numbers.dismissedAlerts), summary.alertsSummary.dismissed)}
            </CardContent>
          </Card>
        </Grid>
      )}

      <Grid item sm={6} md={3} xs={12} className={styles.numberBox}>
        <Card className={styles.card}>
          <CardHeader title={strings(stringKeys.nationalSociety.dashboard.geographicalCoverage)} />
          <CardContent>
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.numbers.numberOfVillages), summary.numberOfVillages)}
            {renderNumber(strings(stringKeys.nationalSociety.dashboard.numbers.numberOfDistricts), summary.numberOfDistricts)}
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
}
