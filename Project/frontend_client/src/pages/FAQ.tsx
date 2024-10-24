import React from 'react';
import { Accordion, AccordionSummary, AccordionDetails, Typography } from '@mui/material';
// import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

interface FAQItemProps {
  question: string;
  answer: string;
}

const FAQItem: React.FC<FAQItemProps> = ({ question, answer }) => (
  <Accordion>
    <AccordionSummary
      // expandIcon={<ExpandMoreIcon />}
      aria-controls="panel-content"
      id="panel-header"
    >
      <Typography>{question}</Typography>
    </AccordionSummary>
    <AccordionDetails>
      <Typography>{answer}</Typography>
    </AccordionDetails>
  </Accordion>
);

interface FAQData {
  question: string;
  answer: string;
}

const FAQ: React.FC = () => {
  const faqs: FAQData[] = [
    {
      question: "How do I create an account?",
      answer: "To create an account, click on the 'Create New Account' link on the login page. Fill in your details and follow the prompts to complete registration."
    },
    {
      question: "What are the password requirements?",
      answer: "We recommend using a strong password that is at least 8 characters long, includes uppercase and lowercase letters, numbers, and special characters."
    },
    {
      question: "How can I reset my password?",
      answer: "If you've forgotten your password, click on the 'Forgot Password' link on the login page. Enter your email address, and we'll send you instructions to reset your password."
    },
    {
      question: "How do I earn points?",
      answer: "You can earn points by driving safely and completing objectives set by your sponsor."
    },
    {
      question: "How do I apply to a sponsor?",
      answer: "To apply to a sponsor, fill out the sponsor application form."
    },
    {
      question: "What can I spend my points on?",
      answer: "Points can be exchanged for purchases from temp not sure where yet."
    }
  ];

  return (
    <div>
      <Typography variant="h4" gutterBottom>Frequently Asked Questions</Typography>
      {faqs.map((faq, index) => (
        <FAQItem key={index} question={faq.question} answer={faq.answer} />
      ))}
    </div>
  );
};

export default FAQ;