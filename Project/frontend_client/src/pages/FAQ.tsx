import React from 'react';
import { Accordion, AccordionSummary, AccordionDetails, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

interface FAQItemProps {
  question: string;
  answer: string;
}

const FAQItem: React.FC<FAQItemProps> = ({ question, answer }) => (
  <Accordion>
    <AccordionSummary
      expandIcon={<ExpandMoreIcon />}
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
      answer: "To create an account, click on the 'Sign Up' Button, and from there you will become a guest."
    },
    {
      question: "What are the password requirements?",
      answer: "To create a password, it must have a number, a lowercase letter, an uppercase letter, be at least 8 characters long and no characters can be repeated."
    },
    {
      question: "How can I reset my password?",
      answer: "If you've forgotten your password, click on the 'Forgot Password' link on the login page."
    },
    {
      question: "How do I earn points?",
      answer: "To earn points, first you must be associated with at least one sponsor. From there, simply follow instructions from your sponsor to earn points."
    },
    {
      question: "How do I apply to a sponsor?",
      answer: "To apply to be a sponsor, simply reach out to us and we will create an account for you if you meet our requirements."
    },
    {
      question: "What can I spend my points on?",
      answer: "You can exchange your points for a variety of items from your sponsors catalog. Each of our sponsors has a unique selection of items."
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