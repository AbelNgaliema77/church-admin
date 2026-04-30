import { Card } from '../../components/ui/Card';

export function SettingsPage() {
  return (
    <>
      <h1 className="page-title">Settings</h1>
      <Card title="System Settings">
        <div className="alert">
          Configure church branding, user access, reporting preferences and operational settings for the live portal.
        </div>
      </Card>
    </>
  );
}
