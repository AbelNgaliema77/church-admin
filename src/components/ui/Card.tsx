import { PropsWithChildren, ReactNode } from 'react';

type Props = PropsWithChildren<{
  title?: string;
  action?: ReactNode;
}>;

export function Card({ title, action, children }: Props) {
  return (
    <section className="card">
      {(title || action) && (
        <div className="card-header">
          {title && <h2>{title}</h2>}
          {action}
        </div>
      )}
      {children}
    </section>
  );
}
