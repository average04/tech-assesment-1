import type { CustomerDto } from '../types/Customer';

interface Props {
  customer: CustomerDto;
  onAddAnother: () => void;
}

export default function ConfirmationCard({ customer, onAddAnother }: Props) {
  return (
    <div className="confirmation">
      <h1>Registration complete</h1>
      <p>
        <strong>{customer.firstName} {customer.lastName}</strong> has been registered.
      </p>
      <ul>
        <li>Email: {customer.email}</li>
        <li>Phone: {customer.phoneNumber}</li>
        <li>Date: {new Date(customer.dateCreated).toLocaleString()}</li>
      </ul>
      <button type="button" onClick={onAddAnother}>Register another</button>
    </div>
  );
}
