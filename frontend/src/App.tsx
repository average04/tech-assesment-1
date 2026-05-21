import { useState } from 'react';
import CustomerForm from './components/CustomerForm';
import ConfirmationCard from './components/ConfirmationCard';
import type { CustomerDto } from './types/Customer';
import './App.css';

export default function App() {
  const [registered, setRegistered] = useState<CustomerDto | null>(null);

  return (
    <main className="container">
      {registered
        ? <ConfirmationCard customer={registered} onAddAnother={() => setRegistered(null)} />
        : <CustomerForm onSuccess={setRegistered} />}
    </main>
  );
}
