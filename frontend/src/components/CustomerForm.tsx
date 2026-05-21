import { useRef, useState } from 'react';
import SignaturePad, { type SignaturePadHandle } from './SignaturePad';
import { createCustomer } from '../api/customerApi';
import type { CustomerDto } from '../types/Customer';

interface Props {
  onSuccess: (customer: CustomerDto) => void;
}

type FieldErrors = Partial<Record<'firstName' | 'lastName' | 'email' | 'phoneNumber' | 'signature' | 'form', string>>;

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^\+?[0-9]{7,15}$/;

export default function CustomerForm({ onSuccess }: Props) {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const padRef = useRef<SignaturePadHandle>(null);

  function validate(): FieldErrors {
    const e: FieldErrors = {};
    if (!firstName.trim()) e.firstName = 'Required';
    else if (firstName.length > 100) e.firstName = 'Max 100 chars';
    if (!lastName.trim()) e.lastName = 'Required';
    else if (lastName.length > 100) e.lastName = 'Max 100 chars';
    if (!email.trim()) e.email = 'Required';
    else if (!EMAIL_RE.test(email)) e.email = 'Invalid email';
    if (!phoneNumber.trim()) e.phoneNumber = 'Required';
    else if (!PHONE_RE.test(phoneNumber)) e.phoneNumber = '7-15 digits, optional +';
    if (padRef.current?.isEmpty()) e.signature = 'Signature required';
    return e;
  }

  async function onSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    const e = validate();
    setErrors(e);
    if (Object.keys(e).length) return;

    setSubmitting(true);
    try {
      const dto = await createCustomer({
        firstName,
        lastName,
        email,
        phoneNumber,
        signatureBase64: padRef.current!.getDataUrl(),
      });
      onSuccess(dto);
    } catch (err: unknown) {
      const msg = (err as { detail?: string }).detail ?? 'Submission failed.';
      setErrors({ form: msg });
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={onSubmit} noValidate>
      <h1>Customer Onboarding</h1>

      <label>
        First name
        <input value={firstName} onChange={(e) => setFirstName(e.target.value)} />
        {errors.firstName && <span className="err">{errors.firstName}</span>}
      </label>

      <label>
        Last name
        <input value={lastName} onChange={(e) => setLastName(e.target.value)} />
        {errors.lastName && <span className="err">{errors.lastName}</span>}
      </label>

      <label>
        Email
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        {errors.email && <span className="err">{errors.email}</span>}
      </label>

      <label>
        Phone number
        <input value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} placeholder="+15551234567" />
        {errors.phoneNumber && <span className="err">{errors.phoneNumber}</span>}
      </label>

      <fieldset>
        <legend>Signature</legend>
        <SignaturePad ref={padRef} />
        <button type="button" onClick={() => padRef.current?.clear()}>Clear</button>
        {errors.signature && <span className="err">{errors.signature}</span>}
      </fieldset>

      {errors.form && <p className="err">{errors.form}</p>}

      <button type="submit" disabled={submitting}>
        {submitting ? 'Submitting…' : 'Register customer'}
      </button>
    </form>
  );
}
