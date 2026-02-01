# Runbook: Azure SQL Restore Drill

This runbook documents how to restore the Azure SQL database for TechContentHub.

## Prerequisites
- Access to the Azure subscription and resource group
- Azure Portal access (or Azure CLI installed)

## Option A: Azure Portal (recommended)
1) Go to **Azure Portal → SQL databases → contenthub**.
2) Select **Backups**.
3) Under **Point-in-time restore**, choose the desired time.
4) Set a new database name (example: `contenthub-restore-YYYYMMDD`).
5) Choose the same server and region.
6) Click **OK** to start the restore.

## Option B: Azure CLI
Replace values in brackets.

```bash
az sql db restore \
  --dest-name contenthub-restore-YYYYMMDD \
  --edition Basic \
  --resource-group <resource-group> \
  --server <sql-server-name> \
  --name <source-db-name> \
  --time "YYYY-MM-DDTHH:MM:SSZ"
```

## Validation checklist
- Connect to the restored DB.
- Run a simple query to confirm schema:
  - Example: `SELECT TOP 1 * FROM ContentItems;`
- Optional: verify recent data exists.

## Cleanup
- Delete the restored database after validation to avoid costs:
  - Azure Portal → SQL databases → `contenthub-restore-YYYYMMDD` → Delete
  - or CLI: `az sql db delete --resource-group <rg> --server <server> --name contenthub-restore-YYYYMMDD`

## Notes
- Automated backups are enabled by default for Azure SQL.
- This drill should be run monthly or before major releases.
