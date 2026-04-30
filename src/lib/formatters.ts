export function formatMoney(amount: number) {
  return amount.toLocaleString('en-ZA', {
    style: 'currency',
    currency: 'ZAR'
  });
}